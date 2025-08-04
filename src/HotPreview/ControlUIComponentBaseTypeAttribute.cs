using System;

namespace HotPreview;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ControlUIComponentBaseTypeAttribute(string platform, string baseType) : Attribute
{
    public string Platform { get; } = platform;

    public string BaseType { get; } = baseType;
}
