namespace PreviewFramework.DevTools.ViewModels.NavTree;

public class PreviewViewModel(Preview preview) : NavTreeItemViewModel
{
    public override string Name => preview.Name;
    public override string Icon => "📄";
    public Preview Preview { get; } = preview;
}
