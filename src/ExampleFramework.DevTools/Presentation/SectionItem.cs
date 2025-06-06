using ExampleFramework.DevTools.Presentation.Models;

namespace ExampleFramework.DevTools.Presentation;

public class SectionItem(string displayName, string icon = "") : TreeItem
{
    public override string DisplayName { get; } = displayName;

    public override string Icon { get; } = icon;

    public override ObservableCollection<TreeItem> Children { get; } = new ObservableCollection<TreeItem>();
}
