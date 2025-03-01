using Microsoft.UIPreview;
using Microsoft.UIPreview.App;
using Microsoft.UIPreview.Maui;
[assembly: PreviewAppService(typeof(MauiPreviewAppService))]

namespace Microsoft.UIPreview.Maui;

public class MauiPreviewAppService : PreviewAppService
{
    public MauiPreviewAppService()
    {
        PreviewNavigatorService = new MauiPreviewNavigatorService();
    }

    public async override Task NavigateToPreviewAsync(string uiComponentName, string previewName)
    {
        PreviewReflection preview = GetPreview(uiComponentName, previewName);
        await PreviewNavigatorService.NavigateToPreviewAsync(preview).ConfigureAwait(false);
    }

    public IPreviewNavigatorService PreviewNavigatorService { get; }

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
