using PreviewFramework.Tooling;

namespace PreviewFramework.DevTools.ViewModels.NavTree;

public class ExampleViewModel(Preview preview) : NavTreeItemViewModel
{
    public override string DisplayName => preview.DisplayName;

    public override string Icon => "";

    public override ObservableCollection<NavTreeItemViewModel>? Children => null;
}
