namespace HotPreview.DevToolsApp.ViewModels.NavTree;

public class SectionItemViewModel(string displayName, string icon = "") : NavTreeItemViewModel
{
    private List<NavTreeItemViewModel> _children = [];

    public override string DisplayName { get; } = displayName;

    public override string PathIcon { get; } = icon;

    public override IReadOnlyList<NavTreeItemViewModel>? Children => _children;

    public void AddChild(NavTreeItemViewModel child)
    {
        _children.Add(child);
    }
}
