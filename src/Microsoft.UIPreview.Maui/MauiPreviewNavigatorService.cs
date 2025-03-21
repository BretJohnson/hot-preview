using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.UIPreview.App;

namespace Microsoft.UIPreview.Maui;

public class MauiPreviewNavigatorService
{
    public bool NavigateAnimationsEnabled { get; set; } = false;

    public virtual void NavigateToPreview(PreviewReflection preview)
    {
        _ = NavigateToPreviewAsync(preview);
    }

    public virtual async Task NavigateToPreviewAsync(PreviewReflection preview)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            object? previewUI = preview.Create();

            //MauiPreviewsApplication.Instance.PrepareToNavigateToPreview();

            if (previewUI is ShellPreview shellPreview)
            {
                await Shell.Current.GoToAsync(shellPreview.Route, NavigateAnimationsEnabled, shellPreview.Parameters);
            }
            else if (previewUI is ContentPage contentPage)
            {
                //MauiPreviewsApplication.Instance.Application.MainPage = contentPage;
                await Application.Current!.MainPage!.Navigation.PushAsync(contentPage, NavigateAnimationsEnabled);
            }
        });
    }
}
