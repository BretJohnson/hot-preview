using Microsoft.PreviewFramework;
using Microsoft.PreviewFramework.App;
using Microsoft.PreviewFramework.Maui;
[assembly: PreviewAppService(typeof(MauiPreviewAppService))]

namespace Microsoft.PreviewFramework.Maui;

public class MauiPreviewAppService : UIPreviewAppService
{
    public MauiPreviewAppService()
    {
        PreviewNavigatorService = new MauiPreviewNavigatorService();
    }

    public async override Task NavigateToPreviewAsync(string uiComponentName, string previewName)
    {
        UIPreviewReflection preview = GetPreview(uiComponentName, previewName);
        await PreviewNavigatorService.NavigateToPreviewAsync(preview).ConfigureAwait(false);
    }

    public IUIPreviewNavigatorService PreviewNavigatorService { get; }

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
