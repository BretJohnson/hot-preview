using Microsoft.CodeAnalysis;

namespace Microsoft.UIPreview.Tooling;

public class PreviewsManager
{
    private UIComponents uiComponents = new UIComponents();

    public PreviewsManager(Compilation compilation)
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
