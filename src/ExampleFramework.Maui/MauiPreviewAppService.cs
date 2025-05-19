using System.Threading.Tasks;
using ExampleFramework.App;

namespace ExampleFramework.Maui;

public class MauiPreviewAppService(MauiExampleApplication mauiPreviewApplication) : ExampleAppService(mauiPreviewApplication)
{
    public override async Task NavigateToPreviewAsync(string uiComponentName, string previewName)
    {
        UIComponentPreviewPairReflection uiComponentPreviewPair = GetUIComponentExamplePair(uiComponentName, previewName);
        await MauiExampleApplication.Instance.PreviewNavigatorService.
            NavigateToPreviewAsync(uiComponentPreviewPair.UIComponent, uiComponentPreviewPair.Example).ConfigureAwait(false);
    }
}
