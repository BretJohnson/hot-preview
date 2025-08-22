using HotPreview.SharedModel;
using HotPreview.Tooling;

namespace HotPreview.DevToolsApp.ViewModels.NavTree;

public class SectionItemViewModel : NavTreeItemViewModel
{
    private readonly MainPageViewModel? _mainPageViewModel;
    private readonly UIComponentCategory? _category;
    private readonly List<NavTreeItemViewModel> _children = [];

    public SectionItemViewModel(string displayName, string icon = "", MainPageViewModel? mainPageViewModel = null, UIComponentCategory? category = null)
    {
        DisplayName = displayName;
        PathIcon = icon;
        _mainPageViewModel = mainPageViewModel;
        _category = category;
    }

    public override string DisplayName { get; }

    public override string PathIcon { get; }

    public override IReadOnlyList<NavTreeItemViewModel>? Children => _children;

    public void AddChild(NavTreeItemViewModel child)
    {
        _children.Add(child);
    }

    public override async Task UpdatePreviewSnapshotsAsync()
    {
        AppManager? appManager = _mainPageViewModel?.CurrentApp;
        if (appManager is not null && _category is not null)
        {
            // Use the new AppManager method to update snapshots for all components in this category
            await appManager.UpdateCategorySnapshotsAsync(_category);
        }
    }
}
