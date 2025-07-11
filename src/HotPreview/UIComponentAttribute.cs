using System;

namespace HotPreview;

/// <summary>
/// An attribute that specifies this class is a UI component. Normally UI compenents don't need
/// to be defined explicitly (defining a preview is sufficient), but this can be used to
/// define a display name for the component.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class UIComponentAttribute : Attribute
{
    public static string TypeFullName => NameUtilities.NormalizeTypeFullName(typeof(UIComponentAttribute));

    /// <summary>
    /// Optional display name for the UI component, if it's different from the type name.
    /// </summary>
    public string? DisplayName { get; }

    public UIComponentAttribute()
    {
    }

    public UIComponentAttribute(string? displayName = null)
    {
        DisplayName = displayName;
    }
}
