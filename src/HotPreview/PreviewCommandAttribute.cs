namespace HotPreview;

/// <summary>
/// An attribute that specifies this is a command that can be executed from the preview tools.
/// Commands are static methods that perform actions and can be invoked from the DevTools UI.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class PreviewCommandAttribute : Attribute
{
    /// <summary>
    /// Optional display name for the command, determining how it appears in navigation UI.
    /// "/" delimiters can be used to indicate hierarchy.
    /// </summary>
    public string? DisplayName { get; }

    public PreviewCommandAttribute()
    {
    }

    public PreviewCommandAttribute(string? displayName = null)
    {
        DisplayName = displayName;
    }
}
