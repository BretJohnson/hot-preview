using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PreviewFramework.Tooling;

public class UIComponentsManager : UIComponentsManagerBase<UIComponent, Preview>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UIComponentsManager"/> class, processing metadata from the
    /// provided compilation and its references to gather UI component information. If/when the compilation
    /// changes, create a new instance of this class to read the new compilation.
    /// </summary>
    /// <param name="compilation">Roslyn compilation</param>
    /// <param name="includeApparentUIComponentsWithNoPreviews">Determines whether to include types that COULD be UIComponents,
    /// because they derive from a UI component class, but don't actually define any previews nor can a preview be constructed
    /// automatically. Can be set by tooling that flags these for the user, to direct them to add a preview.</param>
    public UIComponentsManager(Compilation compilation, bool includeApparentUIComponentsWithNoPreviews = false)
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

    public void AddFromAssemblyAttributes(IAssemblySymbol assemblySymbol)
    {
        ImmutableArray<AttributeData> attributes = assemblySymbol.GetAttributes();
        foreach (AttributeData attribute in attributes)
        {
            string? attributeTypeName = attribute.AttributeClass?.ToDisplayString();
            if (attributeTypeName == PageUIComponentBaseTypeAttribute.TypeFullName)
            {
                AddUIComponentBaseType(attribute, _pageUIComponentBaseTypes);
            }
            else if (attributeTypeName == ControlUIComponentBaseTypeAttribute.TypeFullName)
            {
                AddUIComponentBaseType(attribute, _controlUIComponentBaseTypes);
            }
        }
    }

    public static void AddUIComponentBaseType(AttributeData attribute, UIComponentBaseTypes baseTypes)
    {
        if (attribute.ConstructorArguments.Length == 2)
        {
            if (attribute.ConstructorArguments[0].Value is string platform &&
                attribute.ConstructorArguments[1].Value is string typeName)
            {
                baseTypes.AddBaseType(platform, typeName);
            }
        }
    }

    public UIComponent GetOrAddComponent(string name)
    {
        UIComponent? component = GetUIComponent(name);
        if (component is null)
        {
            component = new UIComponent(UIComponentKind.Page, name);
            AddUIComponent(component);
        }

        return component;
    }

    public void AddPreview(string uiComponentName, Preview preview)
    {
        UIComponent component = GetOrAddComponent(uiComponentName);
        component.AddPreview(preview);
    }

    private class PreviewWalker : CSharpSyntaxWalker
    {
        private readonly Compilation _compilation;
        private readonly SemanticModel _semanticModel;
        private readonly UIComponentsManager _uiComponents;
        private readonly bool _includeApparentUIComponentsWithNoPreviews;

        public PreviewWalker(Compilation compilation, SemanticModel semanticModel, UIComponentsManager uiComponents, bool includeUIComponentsWithNoPreviews)
        {
            _compilation = compilation;
            _semanticModel = semanticModel;
            _uiComponents = uiComponents;
            _includeApparentUIComponentsWithNoPreviews = includeUIComponentsWithNoPreviews;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax methodDeclaration)
        {
            CheckForPreviewMethod(methodDeclaration);
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

            if (uiComponentName is null)
            {
                ITypeSymbol? returnTypeSymbol = _semanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type;
                if (returnTypeSymbol is null)
                {
                    return;
                }

                uiComponentName = returnTypeSymbol.ToDisplayString();
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

            string previewFullName = $"{parentTypeSymbol.ToDisplayString()}.{methodDeclaration.Identifier.Text}";

            var preview = new PreviewStaticMethod(previewFullName, title);
            _uiComponents.AddPreview(uiComponentName, preview);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax classDeclaration)
        {
            CheckForApparentUIComponent(classDeclaration);
            base.VisitClassDeclaration(classDeclaration);
        }

        private void CheckForApparentUIComponent(ClassDeclarationSyntax classDeclaration)
        {
            UIComponentKind uiComponentKind = InferUIComponentKind(classDeclaration);
            if (uiComponentKind == UIComponentKind.Unknown)
            {
                return;
            }

            if (CanHaveAutoGeneratedPreview(classDeclaration))
            {
                INamedTypeSymbol? classTypeSymbol = _semanticModel.GetDeclaredSymbol(classDeclaration);
                if (classTypeSymbol is null)
                {
                    return;
                }

                string uiComponentName = classTypeSymbol.ToDisplayString();

                UIComponent? uiComponent = _uiComponents.GetUIComponent(uiComponentName);
                if (uiComponent is null || uiComponent.Previews.Count == 0)
                {
                    uiComponent ??= _uiComponents.GetOrAddComponent(uiComponentName);

                    var preview = new PreviewClass(uiComponentName, isAutoGenerated: true);
                    _uiComponents.AddPreview(uiComponentName, preview);
                }
            }
            else if (_includeApparentUIComponentsWithNoPreviews)
            {
                INamedTypeSymbol? classTypeSymbol = _semanticModel.GetDeclaredSymbol(classDeclaration);
                if (classTypeSymbol is null)
                {
                    return;
                }

                string uiComponentName = classTypeSymbol.ToDisplayString();
                _uiComponents.GetOrAddComponent(uiComponentName);
            }
        }

        private bool CanHaveAutoGeneratedPreview(ClassDeclarationSyntax classDeclaration)
        {
            if (classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword))
            {
                return false;
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
                    if (_uiComponents.IsUIComponentBaseType(namedBaseType.ToDisplayString(), out UIComponentKind kind))
                    {
                        return kind;
                    }
                }
            }

            return UIComponentKind.Unknown;
        }
    }
}
