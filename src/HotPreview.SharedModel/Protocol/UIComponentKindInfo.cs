using System;

namespace PreviewFramework.SharedModel.Protocol;

public static class UIComponentKindInfo
{
    public const string Page = "page";
    public const string Control = "control";
    public const string Unknown = "unknown";

    /// <summary>
    /// Converts a UIComponentKind enum value to its string representation.
    /// </summary>
    /// <param name="kind">The UIComponentKind enum value to convert</param>
    /// <returns>The string representation of the UIComponentKind</returns>
    public static string FromUIComponentKind(UIComponentKind kind)
    {
        return kind switch
        {
            UIComponentKind.Page => Page,
            UIComponentKind.Control => Control,
            UIComponentKind.Unknown => Unknown,
            _ => throw new ArgumentException($"Unknown UIComponentKind: {kind}", nameof(kind))
        };
    }

    /// <summary>
    /// Converts a string representation to a UIComponentKind enum value.
    /// </summary>
    /// <param name="kindString">The string representation to convert</param>
    /// <returns>The UIComponentKind enum value</returns>
    /// <exception cref="ArgumentException">Thrown when the string does not match any known UIComponentKind</exception>
    public static UIComponentKind ToUIComponentKind(string kindString)
    {
        return kindString switch
        {
            Page => UIComponentKind.Page,
            Control => UIComponentKind.Control,
            Unknown => UIComponentKind.Unknown,
            _ => throw new ArgumentException($"Unknown UIComponentKind string: {kindString}", nameof(kindString))
        };
    }
}
