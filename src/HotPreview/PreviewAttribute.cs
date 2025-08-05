namespace HotPreview;

/// <summary>
/// Specifies that this static method creates a preview for a UI component.
/// </summary>
/// <remarks>
/// <para>The method should return the preview UI object and the Hot Preview platform code will then navigate to that
/// preview. If the method navigates to the preview itself (often true for a single page app) it can return void.</para>
/// <para>The UI component type is automatically inferred from the method return type or, if the method returns void,
/// from the containing class. If you need to explicitly specify a different UI component type, use the generic
/// PreviewAttribute&lt;TUIComponent&gt; instead.</para>
/// </remarks>
/// <param name="displayName">Optional override for the display name of the preview, determining how it appears in the
/// navigation UI. If not specified, the name of the method (or class, for class-based previews) is used, converted to
/// start case (e.g. "MyPreview" becomes "My Preview"). Storybook also uses this start case convention.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class PreviewAttribute(string? displayName = null) : Attribute
{
    /// <summary>
    /// Optional override for the display name of the preview, determining how it appears in the navigation UI.
    /// If not specified, the name of the method (or class, for class-based previews) is used, converted to start case
    /// (e.g. "MyPreview" becomes "My Preview"). Storybook also uses this start case convention.
    /// </summary>
    public string? DisplayName { get; } = displayName;

    public Type? UIComponentType { get; } = null;

    protected PreviewAttribute(string? displayName, Type? uiComponentType) : this(displayName)
    {
        DisplayName = displayName;
        UIComponentType = uiComponentType;
    }
}

/// <summary>
/// Specifies that this static method creates a preview for a UI component with an explicitly specified UI component
/// type.
/// </summary>
/// <remarks>
/// <para>This generic version is only needed when it's necessary to explicitly specify the UI component type. The type
/// parameter TUIComponent specifies the UI component type that this preview is associated with.</para>
/// <para>The method should return the preview UI object and the Hot Preview platform code will then navigate to that
/// preview. If the method navigates to the preview itself (often true for a single page app) it can return void.</para>
/// </remarks>
/// <typeparam name="TUIComponent">The UI component type that this preview is associated with.</typeparam>
/// <param name="displayName">Optional override for the display name of the preview, determining how it appears in the
/// navigation UI. If not specified, the name of the method (or class, for class-based previews) is used, converted to
/// start case (e.g. "MyPreview" becomes "My Preview"). Storybook also uses this same start case convention.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PreviewAttribute<TUIComponent>(string? displayName = null) : PreviewAttribute(displayName, typeof(TUIComponent))
{
}
