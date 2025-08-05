namespace HotPreview;

/// <summary>
/// Controls whether auto-generated previews should be created for a UI component.
/// </summary>
/// <remarks>
/// When present on a class and the autoGenerate property is false, auto-generation is disabled.
/// </remarks>
/// <param name="autoGenerate">Controls whether auto-generated previews should be created for this component. When set to
/// false, auto-generation is disabled.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class AutoGeneratePreviewAttribute(bool autoGenerate) : Attribute
{
    /// <summary>
    /// Controls whether auto-generated previews should be created for this component.
    /// When set to false, auto-generation is disabled.
    /// </summary>
    public bool AutoGenerate { get; } = autoGenerate;
}
