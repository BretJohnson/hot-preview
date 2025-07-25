using HotPreview.Tooling;

namespace HotPreview.DevToolsApp.ViewModels.NavTree;

public class UIComponentViewModel : NavTreeItemViewModel
{
    private readonly MainPageViewModel _mainPageViewModel;

    public UIComponentViewModel(MainPageViewModel mainPageViewModel, UIComponentTooling uiComponent)
    {
        _mainPageViewModel = mainPageViewModel;
        UIComponent = uiComponent;
    }

    public UIComponentTooling UIComponent { get; }

    public override string DisplayName => UIComponent.DisplayName;

    public override string PathIcon => UIComponent.PathIcon;

    public override IReadOnlyList<NavTreeItemViewModel>? Children =>
        UIComponent.HasMultiplePreviews ?
            UIComponent.Previews.Select(preview => new PreviewViewModel(_mainPageViewModel, UIComponent, preview)).ToList() :
            null;

    public override async Task UpdatePreviewSnapshotsAsync()
    {
        AppManager? appManager = _mainPageViewModel.CurrentApp;
        if (appManager is not null)
        {
            await appManager.UpdatePreviewSnapshotsAsync(UIComponent, null);
        }
    }

    public override void OnItemInvoked()
    {
        if (UIComponent.HasSinglePreview)
        {
            // Update status bar to show navigation action
            _mainPageViewModel.UpdateStatusMessage($"Navigating to preview: {UIComponent.DefaultPreview.DisplayName}");

            // Navigate to the preview, for all app connections that have the preview
            _mainPageViewModel.CurrentApp?.NavigateToPreview(UIComponent, UIComponent.DefaultPreview);
        }
    }
}
