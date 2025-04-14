using System.Threading.Tasks;
using Microsoft.UIPreview.App;

namespace Microsoft.UIPreview.Maui;

public class MauiPreviewAppService : PreviewAppService
{
    public override async Task NavigateToPreviewAsync(string uiComponentName, string previewName)
    {
        PreviewReflection preview = GetPreview(uiComponentName, previewName);
        await MauiPreviewApplication.Instance.PreviewNavigatorService.NavigateToPreviewAsync(preview).ConfigureAwait(false);
    }
}
