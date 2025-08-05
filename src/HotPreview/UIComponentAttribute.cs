namespace HotPreview;

/// <summary>
/// Specifies that this class is a UI component.
/// </summary>
/// <remarks>
/// Normally UI components don't need to be defined explicitly (defining a preview is sufficient), but this can
/// be used to define a display name for the component.
/// </remarks>
/// <param name="displayName">Optional override for the display name for the UI component. If not specified, the name
/// of the class is used (with no namespace).</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class UIComponentAttribute(string? displayName = null) : Attribute
{
    public static string TypeFullName => NameUtilities.NormalizeTypeFullName(typeof(UIComponentAttribute));

    /// <summary>
    /// Optional override for the display name for the UI component. If not specified, the name of the class is used
    /// (with no namespace).
    /// </summary>
    public string? DisplayName { get; } = displayName;
}
