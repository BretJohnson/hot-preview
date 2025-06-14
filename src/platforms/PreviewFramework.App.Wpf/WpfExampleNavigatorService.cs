using PreviewFramework.App;

namespace PreviewFramework.App.Wpf;

public class WpfExampleNavigatorService
{
    public virtual void NavigateToExample(UIComponentReflection uiComponent, PreviewReflection preview)
    {
        _ = NavigateToPreviewAsync(uiComponent, preview);
    }

    public virtual async Task NavigateToPreviewAsync(UIComponentReflection uiComponent, PreviewReflection preview)
    {
        throw new NotImplementedException("WpfExampleNavigatorService.NavigateToPreviewAsync is not implemented.");
    }
}
