using Microsoft.PreviewFramework.App;

namespace Microsoft.PreviewFramework.Maui;

public class MauiPreviewNavigatorService : IUIPreviewNavigatorService
{
    public async Task NavigateToPreviewAsync(UIPreviewReflection preview)
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
