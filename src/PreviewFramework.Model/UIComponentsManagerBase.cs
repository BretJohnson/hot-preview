using System;
using System.Collections.Generic;
using System.Linq;

namespace PreviewFramework.Model;

public abstract class UIComponentsManagerBase<TUIComponent, TPreview> where TUIComponent : UIComponentBase<TPreview> where TPreview : PreviewBase
{
    private readonly IReadOnlyDictionary<string, TUIComponent> _uiComponentsByName;
    private readonly IReadOnlyDictionary<string, UIComponentCategory> _categories;
    private List<TUIComponent>? _sortedComponents;

    protected UIComponentsManagerBase(
        IReadOnlyDictionary<string, TUIComponent> uiComponents,
        IReadOnlyDictionary<string, UIComponentCategory> categories)
    {
        _uiComponentsByName = uiComponents;
        _categories = categories;
    }

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

    public TUIComponent? GetUIComponent(string name) =>
        _uiComponentsByName.TryGetValue(name, out TUIComponent? uiComponent) ? uiComponent : null;
}
