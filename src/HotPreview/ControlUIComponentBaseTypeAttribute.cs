using System;

namespace HotPreview;

/// <summary>
/// Specifies the base type for control UI components on a specific platform.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ControlUIComponentBaseTypeAttribute(string platform, string baseType) : Attribute
{
    public string Platform { get; } = platform;

    public string BaseType { get; } = baseType;
}
