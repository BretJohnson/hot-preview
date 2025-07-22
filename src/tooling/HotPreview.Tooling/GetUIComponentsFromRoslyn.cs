using System.Collections.Immutable;
using HotPreview.SharedModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HotPreview.Tooling;

public class GetUIComponentsFromRoslyn : UIComponentsManagerBuilderTooling
{
    /// <summary>
    /// Initializes a new instance of GetUIComponentsFromRoslyn and processes the compilation to gather UI component information.
    /// </summary>
    /// <param name="compilation">Roslyn compilation</param>
    /// <param name="includeApparentUIComponentsWithNoPreviews">Determines whether to include types that COULD be UIComponents,
    /// because they derive from a UI component class, but don't actually define any previews nor can a preview be constructed
    /// automatically. Can be set by tooling that flags these for the user, to direct them to add a preview.</param>
    public GetUIComponentsFromRoslyn(Compilation compilation, bool includeApparentUIComponentsWithNoPreviews)
    {
        IEnumerable<MetadataReference> references = compilation.References;

        // Add the metadata based on assembly attributes -- platform UI component base types and (later) component categories
        foreach (MetadataReference reference in references)
        {
            if (reference is PortableExecutableReference peReference)
            {
                if (compilation.GetAssemblyOrModuleSymbol(peReference) is IAssemblySymbol peAssemblySymbol)
                {
                    AddFromAssemblyAttributes(peAssemblySymbol);
                }
            }
            else if (reference is CompilationReference compilationReference)
            {
                AddFromAssemblyAttributes(compilationReference.Compilation.Assembly);
            }
        }

        AddFromAssemblyAttributes(compilation.Assembly);

        // Later handle component categories, but for now they aren't supported
#if LATER
        /*
        IEnumerable<UIComponentCategoryAttribute> uiComponentCategoryAttributes = assembly.GetCustomAttributes<UIComponentCategoryAttribute>();
        foreach (UIComponentCategoryAttribute uiComponentCategoryAttribute in uiComponentCategoryAttributes)
        {
            UIComponentCategory category = GetOrAddCatgegory(uiComponentCategoryAttribute.Name);

            foreach (Type type in uiComponentCategoryAttribute.UIComponentTypes)
            {
                UIComponent component = GetOrAddUIComponent(type);
                component.SetCategoryFailIfAlreadySet(category);
            }
        }
        */
#endif

        foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);

            var previewWalker = new PreviewWalker(compilation, semanticModel, this, includeApparentUIComponentsWithNoPreviews);

            SyntaxNode root = syntaxTree.GetRoot();
            previewWalker.Visit(root);
        }
    }

    private void AddFromAssemblyAttributes(IAssemblySymbol assemblySymbol)
    {
        ImmutableArray<AttributeData> attributes = assemblySymbol.GetAttributes();
        foreach (AttributeData attribute in attributes)
        {
            string? attributeTypeName = attribute.AttributeClass?.ToDisplayString();
            if (attributeTypeName == PageUIComponentBaseTypeAttribute.TypeFullName)
            {
                AddUIComponentBaseTypeFromAttribute(attribute, UIComponentKind.Page);
            }
            else if (attributeTypeName == ControlUIComponentBaseTypeAttribute.TypeFullName)
            {
                AddUIComponentBaseTypeFromAttribute(attribute, UIComponentKind.Control);
            }
        }
    }

    private void AddUIComponentBaseTypeFromAttribute(AttributeData attribute, UIComponentKind kind)
    {
        if (attribute.ConstructorArguments.Length == 2)
        {
            if (attribute.ConstructorArguments[0].Value is string platform &&
                attribute.ConstructorArguments[1].Value is string typeName)
            {
                AddUIComponentBaseType(kind, platform, typeName);
            }
        }
    }

    private void AddApparentUIElement(string uiComponentName, UIComponentKind kind)
    {
        if (!_uiComponentsByName.TryGetValue(uiComponentName, out UIComponentTooling? component))
        {
            component = new UIComponentTooling(kind, uiComponentName, null, []);
            _uiComponentsByName[uiComponentName] = component;
        }
    }

    private void AddPreview(string uiComponentName, PreviewTooling preview)
    {
        if (_uiComponentsByName.TryGetValue(uiComponentName, out UIComponentTooling? component))
        {
            component = (UIComponentTooling)component.WithAddedPreview(preview);
        }
        else
        {
            // TODO: Add this: UIComponentKind kind = InferUIComponentKind(uiComponentType);
            UIComponentKind kind = UIComponentKind.Page;
            component = new UIComponentTooling(kind, uiComponentName, null, [preview]);
        }

        _uiComponentsByName[uiComponentName] = component;
    }



    private class PreviewWalker(Compilation compilation, SemanticModel semanticModel, GetUIComponentsFromRoslyn builder, bool includeUIComponentsWithNoPreviews) : CSharpSyntaxWalker
    {
        private readonly Compilation _compilation = compilation;
        private readonly SemanticModel _semanticModel = semanticModel;
        private readonly GetUIComponentsFromRoslyn _builder = builder;
        private readonly bool _includeApparentUIComponentsWithNoPreviews = includeUIComponentsWithNoPreviews;

        public override void VisitMethodDeclaration(MethodDeclarationSyntax methodDeclaration)
        {
            CheckForPreviewMethod(methodDeclaration);
            CheckForCommandMethod(methodDeclaration);
            base.VisitMethodDeclaration(methodDeclaration);
        }

        private void CheckForPreviewMethod(MethodDeclarationSyntax methodDeclaration)
        {
            AttributeSyntax? previewAttribute = methodDeclaration.AttributeLists
                .SelectMany(attrList => attrList.Attributes)
                .FirstOrDefault(attr => attr.Name.ToString() == "Preview");
            if (previewAttribute is null)
            {
                return;
            }

            IMethodSymbol? attributeSymbol = _semanticModel.GetSymbolInfo(previewAttribute).Symbol as IMethodSymbol;
            if (attributeSymbol is null)
            {
                return;
            }

            // Verify that the full qualified name of the attribute is correct
            string fullQualifiedAttributeName = attributeSymbol.ContainingType.ToDisplayString();
            if (fullQualifiedAttributeName != PreviewAttribute.TypeFullName)
            {
                return;
            }

            string? uiComponentName = null;
            string? title = null;
            if (previewAttribute.ArgumentList != null)
            {
                SeparatedSyntaxList<AttributeArgumentSyntax> attributeArgs = previewAttribute.ArgumentList.Arguments;

                // If the attribute specifies a example title (1st argument), use it. Otherwise,
                // the title defaults to the method name.
                if (attributeArgs.Count >= 1)
                {
                    AttributeArgumentSyntax firstArgument = attributeArgs[0];
                    if (firstArgument.Expression is LiteralExpressionSyntax literalExpression &&
                        literalExpression.Kind() == SyntaxKind.StringLiteralExpression)
                    {
                        title = literalExpression.Token.ValueText;
                    }
                }

                // If the attribute specifies the UIComponent type, use it. Otherwise, the UIComponent
                // defaults to the method return type
                if (attributeArgs.Count >= 2)
                {
                    AttributeArgumentSyntax secondArgument = attributeArgs[1];
                    if (secondArgument.Expression is TypeOfExpressionSyntax typeOfExpression)
                    {
                        ITypeSymbol? typeSymbol = _semanticModel.GetTypeInfo(typeOfExpression.Type).Type;
                        if (typeSymbol is null)
                        {
                            return;
                        }

                        uiComponentName = typeSymbol.ToDisplayString();
                    }
                }
            }

            if (methodDeclaration.Parent is not TypeDeclarationSyntax typeDeclaration)
            {
                return;
            }

            INamedTypeSymbol? parentTypeSymbol = _semanticModel.GetDeclaredSymbol(typeDeclaration);
            if (parentTypeSymbol is null)
            {
                return;
            }

            if (uiComponentName is null)
            {
                ITypeSymbol? returnTypeSymbol = _semanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type;
                if (returnTypeSymbol is null)
                {
                    return;
                }

                // For a return value of void, use the containing class as the default UI component type
                if (returnTypeSymbol.SpecialType == SpecialType.System_Void)
                {
                    uiComponentName = parentTypeSymbol.ToDisplayString();
                }
                else
                {
                    uiComponentName = returnTypeSymbol.ToDisplayString();
                }
            }

            string previewFullName = $"{parentTypeSymbol.ToDisplayString()}.{methodDeclaration.Identifier.Text}";

            PreviewStaticMethodTooling preview = new PreviewStaticMethodTooling(previewFullName, title);
            _builder.AddPreview(uiComponentName, preview);
        }

        private void CheckForCommandMethod(MethodDeclarationSyntax methodDeclaration)
        {
            AttributeSyntax? commandAttribute = methodDeclaration.AttributeLists
                .SelectMany(attrList => attrList.Attributes)
                .FirstOrDefault(attr => attr.Name.ToString() == "PreviewCommand");
            if (commandAttribute is null)
            {
                return;
            }

            IMethodSymbol? attributeSymbol = _semanticModel.GetSymbolInfo(commandAttribute).Symbol as IMethodSymbol;
            if (attributeSymbol is null)
            {
                return;
            }

            // Verify that the full qualified name of the attribute is correct
            string fullQualifiedAttributeName = attributeSymbol.ContainingType.ToDisplayString();
            if (fullQualifiedAttributeName != PreviewCommandAttribute.TypeFullName)
            {
                return;
            }

            // Validate that the method has no parameters
            if (methodDeclaration.ParameterList.Parameters.Count > 0)
            {
                // Skip methods with parameters - they're not supported yet
                return;
            }

            string? displayName = null;
            if (commandAttribute.ArgumentList != null)
            {
                SeparatedSyntaxList<AttributeArgumentSyntax> attributeArgs = commandAttribute.ArgumentList.Arguments;

                // If the attribute specifies a display name (1st argument), use it
                if (attributeArgs.Count >= 1)
                {
                    AttributeArgumentSyntax firstArgument = attributeArgs[0];
                    if (firstArgument.Expression is LiteralExpressionSyntax literalExpression &&
                        literalExpression.Kind() == SyntaxKind.StringLiteralExpression)
                    {
                        displayName = literalExpression.Token.ValueText;
                    }
                }
            }

            if (methodDeclaration.Parent is not TypeDeclarationSyntax typeDeclaration)
            {
                return;
            }

            INamedTypeSymbol? parentTypeSymbol = _semanticModel.GetDeclaredSymbol(typeDeclaration);
            if (parentTypeSymbol is null)
            {
                return;
            }

            string commandFullName = $"{parentTypeSymbol.ToDisplayString()}.{methodDeclaration.Identifier.Text}";

            PreviewCommandTooling command = new PreviewCommandTooling(commandFullName, displayName);
            _builder.AddOrUpdateCommand(command);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax classDeclaration)
        {
            CheckForApparentUIComponent(classDeclaration);
            base.VisitClassDeclaration(classDeclaration);
        }

        private void CheckForApparentUIComponent(ClassDeclarationSyntax classDeclaration)
        {
            INamedTypeSymbol? classTypeSymbol = _semanticModel.GetDeclaredSymbol(classDeclaration);
            if (classTypeSymbol is null)
            {
                return;
            }

            string uiComponentName = classTypeSymbol.ToDisplayString();

            // Check for UIComponentAttribute first
            CheckForUIComponentAttribute(classDeclaration, classTypeSymbol, uiComponentName);

            UIComponentKind uiComponentKind = InferUIComponentKind(classDeclaration);
            if (uiComponentKind == UIComponentKind.Unknown)
            {
                return;
            }

            if (CanHaveAutoGeneratedPreview(classDeclaration, classTypeSymbol))
            {
                UIComponentTooling? uiComponent = _builder.GetUIComponent(uiComponentName);
                if (uiComponent is null || uiComponent.Previews.Count == 0)
                {
                    _builder.AddPreview(uiComponentName, new PreviewClassTooling(uiComponentName, displayNameOverride: null, isAutoGenerated: true));
                }
            }
            else if (_includeApparentUIComponentsWithNoPreviews)
            {
                _builder.AddApparentUIElement(uiComponentName, uiComponentKind);
            }
        }

        private void CheckForUIComponentAttribute(ClassDeclarationSyntax classDeclaration, INamedTypeSymbol classTypeSymbol, string uiComponentName)
        {
            // Look for UIComponentAttribute on the class
            AttributeSyntax? uiComponentAttribute = classDeclaration.AttributeLists
                .SelectMany(attrList => attrList.Attributes)
                .FirstOrDefault(attr => attr.Name.ToString() == "UIComponent");

            if (uiComponentAttribute is null)
            {
                return;
            }

            IMethodSymbol? attributeSymbol = _semanticModel.GetSymbolInfo(uiComponentAttribute).Symbol as IMethodSymbol;
            if (attributeSymbol is null)
            {
                return;
            }

            // Verify that the full qualified name of the attribute is correct
            string fullQualifiedAttributeName = attributeSymbol.ContainingType.ToDisplayString();
            if (fullQualifiedAttributeName != UIComponentAttribute.TypeFullName)
            {
                return;
            }

            string? displayName = null;
            if (uiComponentAttribute.ArgumentList != null)
            {
                SeparatedSyntaxList<AttributeArgumentSyntax> attributeArgs = uiComponentAttribute.ArgumentList.Arguments;

                // If the attribute specifies a display name (1st argument), use it
                if (attributeArgs.Count >= 1)
                {
                    AttributeArgumentSyntax firstArgument = attributeArgs[0];
                    if (firstArgument.Expression is LiteralExpressionSyntax literalExpression &&
                        literalExpression.Kind() == SyntaxKind.StringLiteralExpression)
                    {
                        displayName = literalExpression.Token.ValueText;
                    }
                }
            }

            UIComponentKind kind = InferUIComponentKind(classTypeSymbol);

            // Add or update the UI component with the display name from the attribute
            UIComponentTooling? existingComponent = _builder.GetUIComponent(uiComponentName);
            if (existingComponent is not null)
            {
                // Update the display name if the component already exists
                UIComponentTooling component = new UIComponentTooling(existingComponent.Kind, uiComponentName, displayName, existingComponent.Previews);
                _builder.AddOrUpdateUIComponent(component);
            }
            else
            {
                // Create a new component with no previews yet
                UIComponentTooling component = new UIComponentTooling(kind, uiComponentName, displayName, []);
                _builder.AddOrUpdateUIComponent(component);
            }
        }

        private bool CanHaveAutoGeneratedPreview(ClassDeclarationSyntax classDeclaration, INamedTypeSymbol classTypeSymbol)
        {
            if (classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword))
            {
                return false;
            }

            // Check if AutoGeneratePreviewAttribute explicitly disables auto-generation
            foreach (AttributeData attribute in classTypeSymbol.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() == "HotPreview.AutoGeneratePreviewAttribute")
                {
                    // Look for the autoGenerate parameter (first parameter)
                    if (attribute.ConstructorArguments.Length >= 1)
                    {
                        TypedConstant autoGenerateArg = attribute.ConstructorArguments[0];
                        if (autoGenerateArg.Value is bool autoGenerate && !autoGenerate)
                        {
                            return false;
                        }
                    }
                    break;
                }
            }

            // Check if the class has a default constructor
            bool hasDefaultConstructor = classDeclaration.Members
                .OfType<ConstructorDeclarationSyntax>()
                .Any(c => c.ParameterList.Parameters.Count == 0 && (c.Modifiers.Any(SyntaxKind.PublicKeyword) || c.Modifiers.Any(SyntaxKind.InternalKeyword)));
            return hasDefaultConstructor;
        }

        private UIComponentKind InferUIComponentKind(ClassDeclarationSyntax classDeclaration)
        {
            string fullTypeName = GetFullClassName(classDeclaration);

            INamedTypeSymbol? typeSymbol = _compilation.GetTypeByMetadataName(fullTypeName);
            if (typeSymbol is null)
            {
                return UIComponentKind.Unknown;
            }

            return InferUIComponentKind(typeSymbol);
        }

        /// <summary>
        /// Gets the fully qualified class name (including namespace) from a ClassDeclarationSyntax
        /// </summary>
        /// <param name="classDeclaration">The ClassDeclarationSyntax to get the full name for</param>
        /// <returns>Fully qualified class name as a string</returns>
        public static string GetFullClassName(ClassDeclarationSyntax classDeclaration)
        {
            // First, check for traditional namespace declaration
            NamespaceDeclarationSyntax? namespaceDeclaration = classDeclaration.Ancestors()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault();
            if (namespaceDeclaration is not null)
            {
                return $"{namespaceDeclaration.Name}.{classDeclaration.Identifier.Text}";
            }

            // Check for file-scoped namespace
            FileScopedNamespaceDeclarationSyntax? fileScoped = classDeclaration.SyntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<FileScopedNamespaceDeclarationSyntax>()
                .FirstOrDefault();
            if (fileScoped is not null)
            {
                return $"{fileScoped.Name}.{classDeclaration.Identifier.Text}";
            }

            // If no namespace is found, return just the class name
            return classDeclaration.Identifier.Text;
        }

        public UIComponentKind InferUIComponentKind(ITypeSymbol from, bool checkImplicitOperator = true)
        {
            // Later check for interfaces, but for now it's not important
#if LATER
            if (to.TypeKind == TypeKind.Interface)
            {
                INamedTypeSymbol toInterface = (INamedTypeSymbol)to;

                // Now check interface equality
                INamedTypeSymbol fromAsNamedSymbol = from as INamedTypeSymbol;
                if (fromAsNamedSymbol != null &&
                    fromAsNamedSymbol.TypeKind == TypeKind.Interface &&
                    toInterface.IsAssignableInterface(fromAsNamedSymbol))
                {
                    return true;
                }

                if (from.AllInterfaces.Any(toInterface.IsAssignableInterface))
                {
                    return true;
                }
            }
#endif

            // Note we don't care about generic classes so we do not do anything special to handle them
            for (ITypeSymbol? baseType = from.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                if (baseType is INamedTypeSymbol namedBaseType)
                {
                    if (_builder.IsUIComponentBaseType(namedBaseType.ToDisplayString(), out UIComponentKind kind))
                    {
                        return kind;
                    }
                }
            }

            return UIComponentKind.Unknown;
        }
    }
}
