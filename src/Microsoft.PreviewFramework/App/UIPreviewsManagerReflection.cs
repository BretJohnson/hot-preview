using System;
using System.Reflection;

namespace Microsoft.PreviewFramework.App;

public class UIPreviewsManagerReflection
{
    private readonly static Lazy<UIPreviewsManagerReflection> s_instance = new Lazy<UIPreviewsManagerReflection> (() =>  new UIPreviewsManagerReflection());
    private readonly UIComponentsReflection _uiComponents;

    public static UIPreviewsManagerReflection Instance => s_instance.Value;

    private UIPreviewsManagerReflection()
    {
        _uiComponents = new UIComponentsReflection();

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            _uiComponents.AddFromAssembly(assembly);
        }
    }

    public UIComponentsReflection UIComponents => _uiComponents;
}
