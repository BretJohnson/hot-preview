using PreviewFramework.Tooling;

namespace PreviewFramework.DevTools.ViewModels.NavTree;

public class UIComponentViewModel(UIComponent uiComponent) : NavTreeItemViewModel
{
    public override string DisplayName => uiComponent.DisplayName;

    public override string Icon => "";

    public override ObservableCollection<NavTreeItemViewModel>? Children { get; } =
        uiComponent.HasMultiplePreviews ?
            new ObservableCollection<NavTreeItemViewModel>(uiComponent.Previews.Select(preview => new PreviewViewModel(preview))) :
            null;

}
