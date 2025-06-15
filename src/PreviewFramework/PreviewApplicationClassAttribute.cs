using System;

namespace PreviewFramework;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class PreviewApplicationClassAttribute(Type previewApplicationClass) : Attribute
{
    public static string TypeFullName { get; } = NameUtilities.NormalizeTypeFullName(typeof(PreviewApplicationClassAttribute));

    public Type PreviewApplicationClass { get; } = previewApplicationClass;
}
