namespace HotPreview;

/// <summary>
/// Use this attribute to specify the category name for a set of UI components. Categories are just used for display purposes.
///
/// If no category is specified for a component, the category name defaults to "Pages" or "Controls", depending on whether the UI component is a page or not.
///
/// This attribute can be specified multiple times for a single category, in which case the UI components are combined together.
/// </summary>
/// <param name="name">The name of the category.</param>
/// <param name="uiComponents">The UI components that belong to this category.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class UIComponentCategoryAttribute(string name, params Type[] uiComponents) : Attribute
{
    public string Name { get; } = name;

    public Type[] UIComponentTypes { get; } = uiComponents;
}
