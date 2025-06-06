using ExampleFramework.DevTools.Presentation.Models;

namespace ExampleFramework.DevTools.Presentation;

public class ExampleItem(string displayName, string icon = "") : TreeItem
{
    public override string DisplayName { get; } = displayName;

    public override string Icon { get; } = icon;

    public override ObservableCollection<TreeItem>? Children => null;
}
