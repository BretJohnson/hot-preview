using System;

namespace Microsoft.UIPreview;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class PreviewApplicationAttribute : Attribute
{
    public Type PreviewApplicationType { get; }

    public PreviewApplicationAttribute(Type previewApplicationType)
    {
        PreviewApplicationType = previewApplicationType;
    }
}
