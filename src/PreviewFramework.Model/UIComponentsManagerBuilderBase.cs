using System;
using System.Collections.Generic;
using System.Linq;

namespace PreviewFramework.Model;

/// <summary>
/// A builder class for constructing UIComponentsManager instances.
/// This class provides mutable operations to build up the state before creating an immutable manager.
/// </summary>
/// <typeparam name="TUIComponent">The type of UI component</typeparam>
/// <typeparam name="TPreview">The type of preview</typeparam>
public class UIComponentsManagerBuilderBase<TUIComponent, TPreview>
    where TUIComponent : UIComponentBase<TPreview>
    where TPreview : PreviewBase
{
    private readonly Dictionary<string, TUIComponent> _uiComponentsByName = [];
    private readonly Dictionary<string, UIComponentCategory> _categories = [];
    private readonly Dictionary<(UIComponentKind kind, string platform), List<string>> _baseTypes = [];

    /// <summary>
    /// Initializes a new instance of the UIComponentsManagerBuilderBase class.
    /// </summary>
    public UIComponentsManagerBuilderBase()
    {
        // Add default MAUI base types
        AddUIComponentBaseType(UIComponentKind.Page, "MAUI", "Microsoft.Maui.Controls.Page");
        AddUIComponentBaseType(UIComponentKind.Control, "MAUI", "Microsoft.Maui.Controls.View");
    }

    public IReadOnlyDictionary<string, TUIComponent> UIComponentsByName => _uiComponentsByName;

    public IReadOnlyDictionary<string, UIComponentCategory> Categories => _categories;

    /// <summary>
    /// Adds a UI component base type for the specified kind and platform.
    /// </summary>
    /// <param name="kind">The kind of UI component (Page or Control)</param>
    /// <param name="platform">The platform name</param>
    /// <param name="baseType">The base type name</param>
    public void AddUIComponentBaseType(UIComponentKind kind, string platform, string baseType)
    {
        if (kind == UIComponentKind.Unknown)
            throw new ArgumentException($"Cannot add base type for unknown UI component kind", nameof(kind));

        (UIComponentKind kind, string platform) key = (kind, platform);
        if (!_baseTypes.TryGetValue(key, out List<string>? baseTypesList))
        {
            baseTypesList = new List<string>();
            _baseTypes[key] = baseTypesList;
        }

        if (!baseTypesList.Contains(baseType))
        {
            baseTypesList.Add(baseType);
        }
    }

    /// <summary>
    /// Adds a UI component to the builder.
    /// </summary>
    /// <param name="component">The component to add</param>
    /// <exception cref="ArgumentException">Thrown when a component with the same name already exists</exception>
    public void AddUIComponent(TUIComponent component)
    {
        if (_uiComponentsByName.ContainsKey(component.Name))
            throw new ArgumentException($"A component with name '{component.Name}' already exists", nameof(component));

        _uiComponentsByName.Add(component.Name, component);
    }

    /// <summary>
    /// Adds or updates a UI component in the builder.
    /// </summary>
    /// <param name="component">The component to add or update</param>
    public void AddOrUpdateUIComponent(TUIComponent component)
    {
        _uiComponentsByName[component.Name] = component;
    }

    /// <summary>
    /// Adds a category to the builder.
    /// </summary>
    /// <param name="category">The category to add</param>
    public void AddCategory(UIComponentCategory category)
    {
        _categories[category.Name] = category;
    }

    /// <summary>
    /// Adds a category by name, creating it if it doesn't exist.
    /// </summary>
    /// <param name="categoryName">The name of the category</param>
    public void AddCategory(string categoryName)
    {
        if (!_categories.ContainsKey(categoryName))
        {
            _categories[categoryName] = new UIComponentCategory(categoryName);
        }
    }

    /// <summary>
    /// Gets or adds a category by name.
    /// </summary>
    /// <param name="categoryName">The name of the category</param>
    /// <returns>The category instance</returns>
    public UIComponentCategory GetOrAddCategory(string categoryName)
    {
        if (!_categories.TryGetValue(categoryName, out UIComponentCategory? category))
        {
            category = new UIComponentCategory(categoryName);
            _categories[categoryName] = category;
        }
        return category;
    }

    /// <summary>
    /// Gets a UI component by name.
    /// </summary>
    /// <param name="name">The name of the component</param>
    /// <returns>The component if found, otherwise null</returns>
    public TUIComponent? GetUIComponent(string name)
    {
        _uiComponentsByName.TryGetValue(name, out TUIComponent? component);
        return component;
    }

    /// <summary>
    /// Checks if a type name is a registered UI component base type.
    /// </summary>
    /// <param name="typeName">The type name to check</param>
    /// <param name="kind">The UI component kind if found</param>
    /// <returns>True if the type is a registered base type, false otherwise</returns>
    public bool IsUIComponentBaseType(string typeName, out UIComponentKind kind)
    {
        foreach (KeyValuePair<(UIComponentKind kind, string platform), List<string>> kvp in _baseTypes)
        {
            if (kvp.Value.Contains(typeName))
            {
                kind = kvp.Key.kind;
                return true;
            }
        }

        kind = UIComponentKind.Unknown;
        return false;
    }

    /// <summary>
    /// Validates the builder state before building.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the builder state is invalid</exception>
    public void Validate()
    {
        // Check for components with categories that don't exist
        foreach (TUIComponent component in _uiComponentsByName.Values)
        {
            if (component.Category != null && !_categories.ContainsKey(component.Category.Name))
            {
                throw new InvalidOperationException(
                    $"Component '{component.Name}' references category '{component.Category.Name}' which doesn't exist in the builder");
            }
        }
    }
}
