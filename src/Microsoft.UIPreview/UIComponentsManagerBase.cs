using System.Collections.Generic;
using System.Linq;

namespace Microsoft.UIPreview;

public abstract class UIComponentsManagerBase<TUIComponent, TPreview> where TUIComponent : UIComponentBase<TPreview> where TPreview : PreviewBase
{
    protected readonly UIComponentBaseTypes _pageUIComponentBaseTypes = new();
    protected readonly UIComponentBaseTypes _controlUIComponentBaseTypes = new();
    protected readonly Dictionary<string, UIComponentCategory> _categories = [];
    protected readonly Dictionary<string, TUIComponent> _uiComponentsByName = [];
    private List<TUIComponent>? _sortedComponents;

    public UIComponentsManagerBase()
    {
        // Certain known platform type are hardcoded here, so they are available by default in more scenarios.
        // Other types can added via the [PageUIComponentBaseType] and [ControlUIComponentBaseType] assembly attributes.
        _pageUIComponentBaseTypes.AddBaseType("MAUI", "Microsoft.Maui.Controls.Page");
        //_controlUIComponentBaseTypes.AddBaseType("MAUI", "Microsoft.Maui.Controls.View");
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
            if (_sortedComponents == null)
            {
                _sortedComponents = _uiComponentsByName.Values.OrderBy(component => component.DisplayName).ToList();
            }
            return _sortedComponents;
        }
    }

    public TUIComponent? GetUIComponent(string name) =>
        _uiComponentsByName.TryGetValue(name, out TUIComponent? uiComponent) ? uiComponent : null;

    public void AddUIComponent(TUIComponent component)
    {
        _uiComponentsByName.Add(component.Name, component);
        _sortedComponents = null;
    }

    public UIComponentCategory GetOrAddCategory(string name)
    {
        if (!_categories.TryGetValue(name, out UIComponentCategory? category))
        {
            category = new UIComponentCategory(name);
            _categories.Add(name, category);
        }

        return category;
    }

    public bool IsUIComponentBaseType(string typeName, out string? platform, out UIComponentKind kind)
    {
        platform = _pageUIComponentBaseTypes.IsUIComponentBaseType(typeName);
        if (platform != null)
        {
            kind = UIComponentKind.Page;
            return true;
        }

        platform = _controlUIComponentBaseTypes.IsUIComponentBaseType(typeName);
        if (platform != null)
        {
            kind = UIComponentKind.Control;
            return true;
        }

        kind = UIComponentKind.Unknown;
        return false;
    }

    public bool IsUIComponentBaseType(string typeName)
    {
        return _pageUIComponentBaseTypes.IsUIComponentBaseType(typeName) != null ||
            _controlUIComponentBaseTypes.IsUIComponentBaseType(typeName) != null;
    }
}
