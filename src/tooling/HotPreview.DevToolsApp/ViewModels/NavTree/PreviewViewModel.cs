using HotPreview.Tooling;
using Microsoft.UI.Xaml.Controls;

namespace HotPreview.DevToolsApp.ViewModels.NavTree;

public class PreviewViewModel : NavTreeItemViewModel
{
    public PreviewViewModel(UIComponentTooling uiComponent, PreviewTooling preview)
    {
        UIComponent = uiComponent;
        Preview = preview;

        UpdateSnapshotsCommand = new RelayCommand(async () =>
        {
            AppManager? appManager = DevToolsManager.Instance.MainPageViewModel.CurrentApp;
            if (appManager is not null)
            {
                await appManager.UpdatePreviewSnapshotsAsync(UIComponent, Preview);
            }
        });
    }

    public UIComponentTooling UIComponent { get; }
    public PreviewTooling Preview { get; }

    public override string DisplayName => Preview.DisplayName;
    public override string PathIcon => UIComponent.PathIcon;

    public override ICommand UpdateSnapshotsCommand { get; }

    public override void OnItemInvoked()
    {
        // Navigate to the preview, for all app connections that have the preview
        DevToolsManager.Instance.MainPageViewModel.CurrentApp?.NavigateToPreview(UIComponent, Preview);
    }
}
