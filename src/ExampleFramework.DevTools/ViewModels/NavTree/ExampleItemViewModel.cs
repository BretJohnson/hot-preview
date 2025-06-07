namespace ExampleFramework.DevTools.ViewModels.NavTree;

public class ExampleItemViewModel(string displayName, string icon = "") : NavTreeItemViewModel
{
    public override string DisplayName { get; } = displayName;

    public override string Icon { get; } = icon;

    public override ObservableCollection<NavTreeItemViewModel>? Children => null;
}
