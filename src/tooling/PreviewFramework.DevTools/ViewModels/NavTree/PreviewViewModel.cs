using PreviewFramework.Tooling;

namespace PreviewFramework.DevTools.ViewModels.NavTree;

public class PreviewViewModel(PreviewTooling preview) : NavTreeItemViewModel
{
    public override string DisplayName => preview.DisplayName;
    public override string Icon => "📄";
    public PreviewTooling Preview => preview;
}
