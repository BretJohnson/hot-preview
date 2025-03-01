using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Microsoft.UIPreview.Tooling;

public class UIComponents : UIComponentsBase<UIComponent, Preview>
{
    public void AddFromRoslynCompilation(Compilation compilation)
    {
        // TODO: Handle component categories
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

        foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);

            var previewWalker = new PreviewWalker(this, semanticModel);

            SyntaxNode root = syntaxTree.GetRoot();
            previewWalker.Visit(root);
        }

        // TODO: Handle this case
        /*
        PreviewAttribute? typePreviewAttribute = type.GetCustomAttribute<PreviewAttribute>(false);
        if (typePreviewAttribute != null)
        {
            AddPreview(new ClassPreview(typePreviewAttribute, type));
        }
        */
    }

    public UIComponent GetOrAddComponent(string name)
    {
        UIComponent? component = this.GetUIComponent(name);
        if (component == null)
        {
            component = new UIComponent(name);
            this.AddUIComponent(component);
        }

        return component;
    }

    public void AddPreview(string uiComponentName, Preview preview)
    {
        UIComponent component = this.GetOrAddComponent(uiComponentName);
        component.AddPreview(preview);
    }

    private class PreviewWalker : CSharpSyntaxWalker
    {
        private UIComponents uiComponents;
        private SemanticModel semanticModel;

        public PreviewWalker(UIComponents uiComponents, SemanticModel semanticModel)
        {

            this.uiComponents = uiComponents;
            this.semanticModel = semanticModel;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax methodDeclaration)
        {
            this.CheckForPreview(methodDeclaration);
            base.VisitMethodDeclaration(methodDeclaration);
        }

        private void CheckForPreview(MethodDeclarationSyntax methodDeclaration)
        {
            AttributeSyntax previewAttribute = methodDeclaration.AttributeLists
                .SelectMany(attrList => attrList.Attributes)
                .FirstOrDefault(attr => attr.Name.ToString() == "Preview");

            if (previewAttribute == null)
            {
                return;
            }

            var attributeSymbol = this.semanticModel.GetSymbolInfo(previewAttribute).Symbol as IMethodSymbol;
            if (attributeSymbol == null)
            {
                return;
            }

            // Verify that the full qualified name of the attribute is correct
            string fullQualifiedAttributeName = attributeSymbol.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (fullQualifiedAttributeName != $"global::{typeof(PreviewAttribute).FullName}")
            {
                return;
            }

            string? uiComponentName = null;
            string? title = null;
            if (previewAttribute.ArgumentList != null)
            {
                SeparatedSyntaxList<AttributeArgumentSyntax> attributeArgs = previewAttribute.ArgumentList.Arguments;

                // If the attribute specifies a preview title (1st argument), use it. Otherwise,
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
                        ITypeSymbol? typeSymbol = this.semanticModel.GetTypeInfo(typeOfExpression.Type).Type;
                        if (typeSymbol == null)
                        {
                            return;
                        }

                        uiComponentName = typeSymbol.ToDisplayString();
                    }
                }
            }

            if (uiComponentName == null)
            {
                ITypeSymbol? returnTypeSymbol = this.semanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type;
                if (returnTypeSymbol == null)
                {
                    return;
                }

                uiComponentName = returnTypeSymbol.ToDisplayString();
            }

            if (methodDeclaration.Parent is not TypeDeclarationSyntax typeDeclaration)
            {
                return;
            }

            INamedTypeSymbol? parentTypeSymbol = this.semanticModel.GetDeclaredSymbol(typeDeclaration);
            if (parentTypeSymbol == null)
            {
                return;
            }

            string previewFullName = $"{parentTypeSymbol.ToDisplayString()}.{methodDeclaration.Identifier.Text}";

            var preview = new PreviewStaticMethod(previewFullName, title);
            this.uiComponents.AddPreview(uiComponentName, preview);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax classDeclaration)
        {
            this.CheckForUIComponent(classDeclaration);
            base.VisitClassDeclaration(classDeclaration);
        }

        private void CheckForUIComponent(ClassDeclarationSyntax classDeclaration)
        {
            if (this.CanBeAutoGeneratedPreview(classDeclaration))
            {
                INamedTypeSymbol? classTypeSymbol = this.semanticModel.GetDeclaredSymbol(classDeclaration);
                if (classTypeSymbol == null)
                {
                    return;
                }

                string uiComponentName = classTypeSymbol.ToDisplayString();

                UIComponent? uiComponent = this.uiComponents.GetUIComponent(uiComponentName);
                if (uiComponent == null || uiComponent.Previews.Count == 0)
                {
                    uiComponent ??= this.uiComponents.GetOrAddComponent(uiComponentName);

                    var preview = new PreviewClass(uiComponentName, isAutoGenerated: true);
                    this.uiComponents.AddPreview(uiComponentName, preview);
                }
            }
        }

        private bool CanBeAutoGeneratedPreview(ClassDeclarationSyntax classDeclaration)
        {
            if (classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword))
            {
                return false;
            }

            // Check if the class has a default constructor
            bool hasDefaultConstructor = classDeclaration.Members
                .OfType<ConstructorDeclarationSyntax>()
                .Any(c => c.ParameterList.Parameters.Count == 0 && (c.Modifiers.Any(SyntaxKind.PublicKeyword) || c.Modifiers.Any(SyntaxKind.InternalKeyword)));
            if (!hasDefaultConstructor)
            {
                return false;
            }

            // TODO: Get fully qualified type for ContentPage & do appropriate check
            // for platform
            // Check if the class derives from ContentPage
            bool derivesFromContentPage = classDeclaration.BaseList?
                .Types
                .Any(t => t.Type.ToString() == "ContentPage") ?? false;
            if (!derivesFromContentPage)
            {
                return false;
            }

            return true;
        }
    }
}
