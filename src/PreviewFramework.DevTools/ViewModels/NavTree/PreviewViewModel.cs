using PreviewFramework.Tooling;

namespace PreviewFramework.DevTools.ViewModels.NavTree;

public class PreviewViewModel(Preview preview) : NavTreeItemViewModel
{
    public override string DisplayName => preview.DisplayName;
    public override string Icon => "📄";
    public Preview Preview => preview;
}
