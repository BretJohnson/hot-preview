namespace HotPreview;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class PageUIComponentBaseTypeAttribute(string platform, string baseType) : Attribute
{
    public static string TypeFullName => NameUtilities.NormalizeTypeFullName(typeof(PageUIComponentBaseTypeAttribute));

    public string Platform { get; } = platform;

    public string BaseType { get; } = baseType;
}
