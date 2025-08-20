using HotPreview.Tooling;

namespace HotPreview.DevToolsApp.ViewModels.NavTree;

public class PreviewViewModel : NavTreeItemViewModel
{
    private readonly MainPageViewModel _mainPageViewModel;

    public PreviewViewModel(MainPageViewModel mainPageViewModel, UIComponentTooling uiComponent, PreviewTooling preview)
    {
        _mainPageViewModel = mainPageViewModel;
        UIComponent = uiComponent;
        Preview = preview;


    }

    public UIComponentTooling UIComponent { get; }
    public PreviewTooling Preview { get; }

    public override string DisplayName => Preview.DisplayName;
    public override string PathIcon => UIComponent.PathIcon;

    public override async Task UpdatePreviewSnapshotsAsync()
    {
        AppManager? appManager = _mainPageViewModel.CurrentApp;
        if (appManager is not null)
        {
            await appManager.UpdateSnapshotsAsync(UIComponent, Preview);
        }
    }

    public override void OnItemInvoked()
    {
        // Update status bar to show navigation action
        _mainPageViewModel.UpdateStatusMessage($"Navigating to preview: {Preview.DisplayName}");

        // Navigate to the preview, for all app connections that have the preview
        _mainPageViewModel.CurrentApp?.NavigateToPreview(UIComponent, Preview);
    }
}
