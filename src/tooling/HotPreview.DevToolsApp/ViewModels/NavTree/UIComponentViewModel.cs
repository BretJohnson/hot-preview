using HotPreview.Tooling;

namespace HotPreview.DevToolsApp.ViewModels.NavTree;

public class UIComponentViewModel : NavTreeItemViewModel
{
    public UIComponentViewModel(UIComponentTooling uiComponent)
    {
        UIComponent = uiComponent;

        UpdateSnapshotsCommand = new RelayCommand(async () =>
        {
            AppManager? appManager = DevToolsManager.Instance.MainPageViewModel.CurrentApp;
            if (appManager is not null)
            {
                await appManager.UpdatePreviewSnapshotsAsync(UIComponent, null);
            }
        });
    }

    public UIComponentTooling UIComponent { get; }

    public override string DisplayName => UIComponent.DisplayName;

    public override string PathIcon => UIComponent.PathIcon;

    public override IReadOnlyList<NavTreeItemViewModel>? Children =>
        UIComponent.HasMultiplePreviews ?
            UIComponent.Previews.Select(preview => new PreviewViewModel(UIComponent, preview)).ToList() :
            null;

    public override ICommand UpdateSnapshotsCommand { get; }

    public override void OnItemInvoked()
    {
        if (UIComponent.HasSinglePreview)
        {
            // Navigate to the preview, for all app connections that have the preview
            DevToolsManager.Instance.MainPageViewModel.CurrentApp?.NavigateToPreview(UIComponent, UIComponent.DefaultPreview);
        }
    }
}
