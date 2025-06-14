using PreviewFramework.App;

namespace PreviewFramework.App.Wpf;

public class WpfPreviewAppService(WpfPreviewApplication wpfPreviewApplication) : PreviewAppService(wpfPreviewApplication)
{
    public override async Task NavigateToPreviewAsync(string uiComponentName, string previewName)
    {
        UIComponentPreviewPairReflection uiComponentPreviewPair = GetUIComponentPreviewPair(uiComponentName, previewName);
        await WpfPreviewApplication.Instance.ExampleNavigatorService.
            NavigateToPreviewAsync(uiComponentPreviewPair.UIComponent, uiComponentPreviewPair.Preview).ConfigureAwait(false);
    }
}
