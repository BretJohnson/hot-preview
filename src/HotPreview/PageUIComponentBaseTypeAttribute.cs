using System;

namespace HotPreview;

/// <summary>
/// Specifies the base type for page UI components on a specific platform.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class PageUIComponentBaseTypeAttribute(string platform, string baseType) : Attribute
{
    public string Platform { get; } = platform;

    public string BaseType { get; } = baseType;
}
