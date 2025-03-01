using Microsoft.UIPreview.App;

namespace Microsoft.UIPreview.Maui;

public class MauiPreviewNavigatorService : IPreviewNavigatorService
{
    public async Task NavigateToPreviewAsync(PreviewReflection preview)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            object? previewUI = preview.Create();

            //MauiPreviewsApplication.Instance.PrepareToNavigateToPreview();

            if (previewUI is ShellPreview shellPreview)
            {
                await Shell.Current.GoToAsync(shellPreview.Route, animate:false, shellPreview.Parameters);
            }
            else if (previewUI is ContentPage contentPage)
            {
                //MauiPreviewsApplication.Instance.Application.MainPage = contentPage;
                await Application.Current.MainPage.Navigation.PushAsync(contentPage);
            }
        });
    }
}
