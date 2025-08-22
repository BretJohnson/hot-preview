using HotPreview.Tooling;

namespace HotPreview.DevToolsApp.ViewModels.NavTree;

public class UIComponentViewModel : NavTreeItemViewModel
{
    private readonly MainPageViewModel _mainPageViewModel;
    private IReadOnlyList<NavTreeItemViewModel>? _children;

    public UIComponentViewModel(MainPageViewModel mainPageViewModel, UIComponentTooling uiComponent)
    {
        _mainPageViewModel = mainPageViewModel;
        UIComponent = uiComponent;
    }

    public UIComponentTooling UIComponent { get; }

    public override string DisplayName => UIComponent.DisplayName;

    public override string PathIcon => UIComponent.PathIcon;

    public override IReadOnlyList<NavTreeItemViewModel>? Children
    {
        get
        {
            if (_children is null && UIComponent.HasMultiplePreviews)
            {
                _children = UIComponent.Previews.Select(preview => new PreviewViewModel(_mainPageViewModel, UIComponent, preview)).ToList();
            }
            return _children;
        }
    }

    public override async Task UpdatePreviewSnapshotsAsync()
    {
        AppManager? appManager = _mainPageViewModel.CurrentApp;
        if (appManager is not null)
        {
            await appManager.UpdateSnapshotsAsync(UIComponent, null);
        }
    }

    public override void OnItemInvoked()
    {
        if (UIComponent.HasSinglePreview)
        {
            // Update status bar to show navigation action
            _mainPageViewModel.UpdateStatusMessage($"Navigating to preview: {UIComponent.DisplayName}");

            // Navigate to the preview, for all app connections that have the preview
            _mainPageViewModel.CurrentApp?.NavigateToPreview(UIComponent, UIComponent.DefaultPreview);
        }
    }
}
