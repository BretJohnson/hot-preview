using PreviewFramework.Tooling;

namespace PreviewFramework.DevToolsApp.ViewModels.NavTree;

public class UIComponentViewModel(UIComponentTooling uiComponent) : NavTreeItemViewModel
{
    public override string DisplayName => uiComponent.DisplayName;

    public override string PathIcon => uiComponent.PathIcon;

    public override IReadOnlyList<NavTreeItemViewModel>? Children { get; } =
        uiComponent.HasMultiplePreviews ?
            uiComponent.Previews.Select(preview => new PreviewViewModel(uiComponent, preview)).ToList() :
            null;

    public override void OnItemInvoked()
    {
        if (uiComponent.HasSinglePreview)
        {
            // Navigate to the preview, for all app connections that have the preview
            DevToolsManager.Instance.MainPageViewModel.CurrentApp?.NavigateToPreview(uiComponent, uiComponent.DefaultPreview);
        }
    }
}
