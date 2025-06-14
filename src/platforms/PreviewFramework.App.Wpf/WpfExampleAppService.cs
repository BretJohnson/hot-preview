using PreviewFramework.App;

namespace PreviewFramework.App.Wpf;

public class WpfExampleAppService(WpfExampleApplication wpfExampleApplication) : ExampleAppService(wpfExampleApplication)
{
    public override async Task NavigateToExampleAsync(string uiComponentName, string exampleName)
    {
        UIComponentPreviewPairReflection uiComponentPreviewPair = GetUIComponentPreviewPair(uiComponentName, previewName);
        await WpfExampleApplication.Instance.ExampleNavigatorService.
            NavigateToExampleAsync(uiComponentExamplePair.UIComponent, uiComponentExamplePair.Example).ConfigureAwait(false);
    }
}
