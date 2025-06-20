using System.Threading.Tasks;
using PreviewFramework.Model.App;

namespace PreviewFramework.App.Maui;

public class MauiPreviewAppService(MauiPreviewApplication mauiPreviewApplication) : PreviewAppService(mauiPreviewApplication)
{
    public override async Task NavigateToPreviewAsync(string uiComponentName, string previewName)
    {
        UIComponentPreviewPairReflection uiComponentPreviewPair = GetUIComponentPreviewPair(uiComponentName, previewName);
        await MauiPreviewApplication.Instance.PreviewNavigatorService.
            NavigateToPreviewAsync(uiComponentPreviewPair.UIComponent, uiComponentPreviewPair.Preview).ConfigureAwait(false);
    }
}
