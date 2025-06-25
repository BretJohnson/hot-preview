using System.Collections.Generic;
using System.Linq;

namespace PreviewFramework.SharedModel;

public abstract class UIComponentsManagerBase<TUIComponent, TPreview>(
    IReadOnlyDictionary<string, TUIComponent> uiComponents,
    IReadOnlyDictionary<string, UIComponentCategory> categories) where TUIComponent : UIComponentBase<TPreview> where TPreview : PreviewBase
{
    private readonly IReadOnlyDictionary<string, TUIComponent> _uiComponentsByName = uiComponents;
    private readonly IReadOnlyDictionary<string, UIComponentCategory> _categories = categories;
    private IReadOnlyList<TUIComponent>? _sortedComponents;
    private IReadOnlyList<(UIComponentCategory Category, IReadOnlyList<TUIComponent> UIComponents)>? _categorizedUIComponents;

    public IEnumerable<UIComponentCategory> Categories => _categories.Values;

    public IEnumerable<TUIComponent> UIComponents => _uiComponentsByName.Values;

    /// <summary>
    /// Get all the UI components, sorted alphabetically by display name.
    /// </summary>
    public IReadOnlyList<TUIComponent> SortedUIComponents
    {
        get
        {
            if (_sortedComponents is null)
            {
                _sortedComponents = _uiComponentsByName.Values.OrderBy(component => component.DisplayName).ToList();
            }
            return _sortedComponents;
        }
    }

    /// <summary>
    /// Get UI components grouped by category, sorted by category name, with UI components sorted by display name.
    /// Includes an "Uncategorized" category for components not in any other category.
    /// </summary>
    public IReadOnlyList<(UIComponentCategory Category, IReadOnlyList<TUIComponent> UIComponents)> CategorizedUIComponents
    {
        get
        {
            if (_categorizedUIComponents is null)
            {
                var result = new List<(UIComponentCategory Category, IReadOnlyList<TUIComponent> UIComponents)>();

                // Add existing categories
                IEnumerable<(UIComponentCategory category, IReadOnlyList<TUIComponent> categoryUIComponents)> categorizedResult = _categories.Values
                    .Select(category =>
                    {
                        IReadOnlyList<TUIComponent> categoryUIComponents = category.UIComponentNames
                            .Select(name => GetUIComponent(name))
                            .OfType<TUIComponent>()  // Filters out nulls
                            .OrderBy(component => component.DisplayName)
                            .ToList();
                        return (category, categoryUIComponents);
                    });

                result.AddRange(categorizedResult);

                // Add uncategorized components
                var categorizedComponentNames = new HashSet<string>(_categories.Values
                    .SelectMany(category => category.UIComponentNames));

                IReadOnlyList<TUIComponent> uncategorizedComponents = _uiComponentsByName.Values
                    .Where(component => !categorizedComponentNames.Contains(component.Name))
                    .OrderBy(component => component.DisplayName)
                    .ToList();

                if (uncategorizedComponents.Count > 0)
                {
                    var uncategorizedCategory = new UIComponentCategory("Uncategorized", []);
                    result.Add((uncategorizedCategory, uncategorizedComponents));
                }

                // Sort all categories by name
                _categorizedUIComponents = result
                    .OrderBy(item => item.Category.Name)
                    .ToList();
            }

            return _categorizedUIComponents;
        }
    }

    public bool HasCategories => _categories.Count > 0;

    public TUIComponent? GetUIComponent(string name) =>
        _uiComponentsByName.TryGetValue(name, out TUIComponent? uiComponent) ? uiComponent : null;

    /// <summary>
    /// Returns true if the manager contains the specified UI component and that component contains the specified preview.
    /// </summary>
    /// <param name="uiComponentName">The name of the UI component to check.</param>
    /// <param name="previewName">The name of the preview to check.</param>
    /// <returns>True if both the UI component and preview exist; otherwise, false.</returns>
    public bool HasPreview(string uiComponentName, string previewName)
    {
        TUIComponent? uiComponent = GetUIComponent(uiComponentName);
        return uiComponent?.GetPreview(previewName) is not null;
    }
}
