using HotPreview.Tooling;
using Microsoft.UI.Xaml.Controls;

namespace HotPreview.DevToolsApp.ViewModels.NavTree;

public class PreviewViewModel : NavTreeItemViewModel
{
    private readonly MainPageViewModel _mainPageViewModel;

    public PreviewViewModel(MainPageViewModel mainPageViewModel, UIComponentTooling uiComponent, PreviewTooling preview)
    {
        _mainPageViewModel = mainPageViewModel;
        UIComponent = uiComponent;
        Preview = preview;

        UpdateSnapshotsCommand = new RelayCommand(async () =>
        {
            AppManager? appManager = _mainPageViewModel.CurrentApp;
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
        _mainPageViewModel.CurrentApp?.NavigateToPreview(UIComponent, Preview);
    }
}
