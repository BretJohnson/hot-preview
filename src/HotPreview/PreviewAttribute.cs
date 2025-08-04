namespace HotPreview;

/// <summary>
/// An attribute that specifies this method (which must be static) creates a preview for a UI component.
///
/// The method should return the preview UI object and the Hot Preview platform code
/// will then navigate to that preview. If the method navigates to the preview itself (often true
/// for a single page app) it can return void.
///
/// The UI component type is automatically inferred from the method return type or, if the method
/// returns void, from the containing class. If you need to explicitly specify a different UI
/// component type, use the generic PreviewAttribute&lt;TUIComponent&gt; instead.
/// </summary>
/// <param name="displayName">Optional override for the display name of the preview, determining how it appears in the navigation UI.
/// If not specified, the name of the method (or class, for class-based previews) is used, converted to start case
/// (e.g. "MyPreview" becomes "My Preview"). Storybook also uses this start case convention.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PreviewAttribute(string? displayName = null) : Attribute
{
    public static string TypeFullName => NameUtilities.NormalizeTypeFullName(typeof(PreviewAttribute));

    /// <summary>
    /// Optional override for the display name of the preview, determining how it appears in the navigation UI.
    /// If not specified, the name of the method (or class, for class-based previews) is used, converted to
    /// start case (e.g. "MyPreview" becomes "My Preview"). Storybook also uses this start case convention.
    /// </summary>
    public string? DisplayName { get; } = displayName;
}

/// <summary>
/// An attribute that specifies this method (which must be static) creates a preview for a UI component,
/// with an explicitly specified UI component type.
///
/// This generic version is only needed when it's necessary to explicitly specify the UI component type.
/// The type parameter TUIComponent specifies the UI component type that this preview is associated with.
///
/// The method should return the preview UI object and the Hot Preview platform code
/// will then navigate to that preview. If the method navigates to the preview itself (often true
/// for a single page app) it can return void.
/// </summary>
/// <typeparam name="TUIComponent">The UI component type that this preview is associated with.</typeparam>
/// <param name="displayName">Optional override for the display name of the preview, determining how it appears in the navigation UI.
/// If not specified, the name of the method (or class, for class-based previews) is used, converted to start case
/// (e.g. "MyPreview" becomes "My Preview"). Storybook also uses this same start case convention.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PreviewAttribute<TUIComponent>(string? displayName = null) : Attribute
{
    public static string TypeFullName => NameUtilities.NormalizeTypeFullName(typeof(PreviewAttribute));

    /// <summary>
    /// Optional override for the display name of the preview, determining how it appears in the navigation UI.
    /// If not specified, the name of the method (or class, for class-based previews) is used, converted to
    /// start case (e.g. "MyPreview" becomes "My Preview"). Storybook also uses this start case convention.
    /// </summary>
    public string? DisplayName { get; } = displayName;

    public Type UIComponentType { get; } = typeof(TUIComponent);
}
