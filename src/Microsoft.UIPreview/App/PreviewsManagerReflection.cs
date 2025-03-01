using System;
using System.Reflection;

namespace Microsoft.UIPreview.App;

public class PreviewsManagerReflection
{
    private readonly static Lazy<PreviewsManagerReflection> s_instance = new Lazy<PreviewsManagerReflection> (() =>  new PreviewsManagerReflection());
    private readonly UIComponentsReflection _uiComponents;

    public static PreviewsManagerReflection Instance => s_instance.Value;

    private PreviewsManagerReflection()
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
