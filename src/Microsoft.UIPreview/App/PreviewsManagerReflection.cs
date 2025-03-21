using System;
using System.Reflection;

namespace Microsoft.UIPreview.App;

public class PreviewsManagerReflection
{
    private static readonly Lazy<PreviewsManagerReflection> s_instance = new Lazy<PreviewsManagerReflection>(() => new PreviewsManagerReflection());

    public static PreviewsManagerReflection Instance => s_instance.Value;

    public UIComponentsReflection UIComponents { get; }

    private PreviewsManagerReflection()
    {
        UIComponents = new UIComponentsReflection();

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            UIComponents.AddFromAssembly(assembly);
        }
    }
}
