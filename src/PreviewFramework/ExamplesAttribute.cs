using System;

namespace PreviewFramework;

/// <summary>
/// An attribute that specifies metadata for UI component that has previews.
/// It can be used explicitly specify a Title, overriding the default title
/// of the type name.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class PreviewssAttribute : Attribute
{
    /// <summary>
    /// Optional title for the preview, determining how it appears in navigation UI.
    /// "/" delimiters can be used to indicate hierarchy.
    /// </summary>
    public string? Title { get; }

    public PreviewssAttribute()
    {
    }

    public PreviewssAttribute(string title)
    {
        this.Title = title;
    }
}
