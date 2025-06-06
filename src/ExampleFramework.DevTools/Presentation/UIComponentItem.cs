using ExampleFramework.DevTools.Presentation.Models;

namespace ExampleFramework.DevTools.Presentation;

public class UIComponentItem : TreeItem
{
    public UIComponentItem(string displayName, string icon = "", ObservableCollection<TreeItem>? children = null)
    {
        DisplayName = displayName;
        Icon = icon;
        Children = children;
    }

    public override string DisplayName { get; }
    public override string Icon { get; }
    public override ObservableCollection<TreeItem>? Children { get; }
}
