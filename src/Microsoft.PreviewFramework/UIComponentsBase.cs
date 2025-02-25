using System.Collections.Generic;
using System.Linq;

namespace Microsoft.PreviewFramework;

public abstract class UIComponentsBase<TUIComponent, TPreview> where TUIComponent : UIComponentBase<TPreview> where TPreview : UIPreviewBase
{
    protected readonly Dictionary<string, UIComponentCategory> categories = new();
    protected readonly Dictionary<string, TUIComponent> componentsByName = new();
    private List<TUIComponent>? sortedComponents;

    public IEnumerable<UIComponentCategory> Categories => this.categories.Values;

    public IEnumerable<TUIComponent> Components => this.componentsByName.Values;

    /// <summary>
    /// Get all the UI components, sorted alphabetically by display name.
    /// </summary>
    public IReadOnlyList<TUIComponent> SortedComponents
    {
        get
        {
            if (this.sortedComponents == null)
            {
                this.sortedComponents = this.componentsByName.Values.OrderBy(component => component.DisplayName).ToList();
            }
            return this.sortedComponents;
        }
    }

    public TUIComponent? GetUIComponent(string name) =>
        this.componentsByName.TryGetValue(name, out TUIComponent? uiComponent) ? uiComponent : null;

    public void AddUIComponent(TUIComponent component)
    {
        this.componentsByName.Add(component.Name, component);
        this.sortedComponents = null;
    }

    public UIComponentCategory GetOrAddCategory(string name)
    {
        if (!this.categories.TryGetValue(name, out UIComponentCategory? category))
        {
            category = new UIComponentCategory(name);
            this.categories.Add(name, category);
        }

        return category;
    }
}
