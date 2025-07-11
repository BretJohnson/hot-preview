using HotPreview.Tooling;

namespace HotPreview.DevToolsApp.ViewModels.NavTree;

public class PreviewViewModel(UIComponentTooling uiComponent, PreviewTooling preview) : NavTreeItemViewModel
{
    public UIComponentTooling UIComponent { get; } = uiComponent;
    public PreviewTooling Preview { get; } = preview;

    public override string DisplayName => Preview.DisplayName;
    public override string PathIcon => UIComponent.PathIcon;

    public override void OnItemInvoked()
    {
        // Navigate to the preview, for all app connections that have the preview
        DevToolsManager.Instance.MainPageViewModel.CurrentApp?.NavigateToPreview(UIComponent, Preview);
    }
}
