using System;

namespace HotPreview;

/// <summary>
/// An attribute that controls whether auto-generated previews should be created for a UI component.
/// When present on a class and the autoGenerate property is false, auto-generation is disabled.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class AutoGeneratePreviewAttribute : Attribute
{
    public static string TypeFullName => NameUtilities.NormalizeTypeFullName(typeof(AutoGeneratePreviewAttribute));

    /// <summary>
    /// Controls whether auto-generated previews should be created for this component.
    /// When set to false, auto-generation is disabled.
    /// </summary>
    public bool AutoGenerate { get; }

    public AutoGeneratePreviewAttribute(bool autoGenerate)
    {
        AutoGenerate = autoGenerate;
    }
}
