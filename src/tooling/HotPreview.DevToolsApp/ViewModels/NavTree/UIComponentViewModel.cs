using HotPreview.Tooling;

namespace HotPreview.DevToolsApp.ViewModels.NavTree;

public class UIComponentViewModel(MainPageViewModel mainPageViewModel, UIComponentTooling uiComponent) : NavTreeItemViewModel
{
    private readonly MainPageViewModel _mainPageViewModel = mainPageViewModel;
    private IReadOnlyList<NavTreeItemViewModel>? _children;

    public UIComponentTooling UIComponent { get; } = uiComponent;

    public override string DisplayName => UIComponent.DisplayName;

    public override string PathIcon => UIComponent.PathIcon;

    public override string? ToolTipText
    {
        get
        {
            string fullName = UIComponent.Name;
            string? displayOverride = UIComponent.DisplayNameOverride;

            string tooltip = !string.IsNullOrWhiteSpace(displayOverride)
                ? $"{fullName} [{displayOverride}]"
                : fullName;

            // If there is exactly one preview and its name differs from the component name,
            // add an extra line showing the preview's full name.
            if (UIComponent.HasSinglePreview)
            {
                PreviewTooling defaultPreview = UIComponent.DefaultPreview;
                if (defaultPreview.Name != UIComponent.Name)
                {
                    tooltip += "\nPreview: " + defaultPreview.Name;
                }
            }

            return tooltip;
        }
    }

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
