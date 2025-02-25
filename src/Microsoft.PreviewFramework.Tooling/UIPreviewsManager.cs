using Microsoft.CodeAnalysis;

namespace Microsoft.PreviewFramework.Tooling;

public class UIPreviewsManager
{
    private UIComponents uiComponents = new UIComponents();

    public UIPreviewsManager(Compilation compilation)
    {
        this.uiComponents = new UIComponents();

#if false
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            _uiComponents.AddFromAssembly(assembly);
        }
#endif
    }

    public UIComponents UIComponents => this.uiComponents;
}
