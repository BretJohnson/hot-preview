using System;
using System.Reflection;

namespace Microsoft.PreviewFramework.App;

public class UIPreviewStaticMethodReflection : UIPreviewReflection
{
    public MethodInfo MethodInfo { get; }

    public UIPreviewStaticMethodReflection(PreviewAttribute previewAttribute, MethodInfo methodInfo) : base(previewAttribute)
    {
        MethodInfo = methodInfo;
    }

    public override object Create()
    {
        if (MethodInfo.GetParameters().Length != 0)
            throw new InvalidOperationException($"Previews that take parameters aren't yet supported: {Name}");

        return MethodInfo.Invoke(null, null);
    }

    public override Type? DefaultUIComponentType => MethodInfo.ReturnType;

    /// <summary>
    /// FullName is intended to be what's used by the code to identify the preview. It's the preview's
    /// full qualified method name.
    /// </summary>
    public override string Name => MethodInfo.DeclaringType.FullName + "." + MethodInfo.Name;
}
