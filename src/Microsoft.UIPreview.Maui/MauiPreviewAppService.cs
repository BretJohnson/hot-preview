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

#if LATER
    public async override Task<ImageSnapshot> GetPreviewSnapshotAsync(string uiComponentName, string previewName)
    {
        PreviewReflection preview = GetPreview(uiComponentName, previewName);

        if (Application.Current?.MainPage is not RemoteControlMainPage remoteControlMainPage)
            throw new InvalidOperationException("MainPage isn't a RemoteControlMainPage");

        return await remoteControlMainPage.GetPreviewSnapshotAsync(preview);
    }
#endif
}
