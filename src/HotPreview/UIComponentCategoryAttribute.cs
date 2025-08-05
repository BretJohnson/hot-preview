namespace HotPreview;

/// <summary>
/// Specifies the category name for a set of UI components.
/// </summary>
/// <remarks>
/// <para>Categories are just used for display purposes.</para>
/// <para>If no category is specified for a component, the category name defaults to "Pages" or "Controls", depending
/// on whether the UI component is a page or not.</para>
/// <para>This attribute can be specified multiple times for a single category, in which case the UI components are
/// combined together.</para>
/// </remarks>
/// <param name="name">The name of the category.</param>
/// <param name="uiComponents">The UI components that belong to this category.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class UIComponentCategoryAttribute(string name, params Type[] uiComponents) : Attribute
{
    public string Name { get; } = name;

    public Type[] UIComponentTypes { get; } = uiComponents;
}
