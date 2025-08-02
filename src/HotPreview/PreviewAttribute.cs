using System;

namespace HotPreview;

/// <summary>
/// An attribute that specifies this is a preview, for a control or other UI.
/// Previews can be shown in a gallery viewer, doc, etc.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PreviewAttribute : Attribute
{
    public static string TypeFullName => NameUtilities.NormalizeTypeFullName(typeof(PreviewAttribute));

    /// <summary>
    /// Optional title for the preview, determining how it appears in navigation UI.
    /// "/" delimiters can be used to indicate hierarchy.
    /// </summary>
    public string? DisplayName { get; }

    public Type? UIComponentType { get; }

    public PreviewAttribute()
    {
    }

    public PreviewAttribute(string? displayName = null, Type? uiComponent = null)
    {
        DisplayName = displayName;
        UIComponentType = uiComponent;
    }

    public PreviewAttribute(Type uiComponent)
    {
        UIComponentType = uiComponent;
    }
}
