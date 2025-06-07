namespace ExampleFramework.DevTools.ViewModels.NavTree;

public class UIComponentItemViewModel(string displayName, string icon = "", ObservableCollection<NavTreeItemViewModel>? children = null) : NavTreeItemViewModel
{
    public override string DisplayName { get; } = displayName;
    public override string Icon { get; } = icon;
    public override ObservableCollection<NavTreeItemViewModel>? Children { get; } = children;
}
