using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using ExampleFramework.App;

namespace ExampleFramework.Maui;

public class MauiPreviewNavigatorService
{
    public bool NavigateAnimationsEnabled { get; set; } = false;

    public virtual void NavigateToPreview(UIComponentReflection uiComponent, ExampleReflection preview)
    {
        _ = NavigateToPreviewAsync(uiComponent, preview);
    }

    public virtual async Task NavigateToPreviewAsync(UIComponentReflection uiComponent, ExampleReflection preview)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            object? previewUI = preview.Create();

            if (uiComponent.Kind == UIComponentKind.Control)
            {
                ContentPage controlsPage = new ContentPage
                {
                    Content = (View)previewUI
                };

                await Application.Current!.MainPage!.Navigation.PushAsync(controlsPage, NavigateAnimationsEnabled);
            }
            else
            {
                if (previewUI is RoutePreview shellPreview)
                {
                    Window? mainWindow = Application.Current!.Windows[0];
                    Shell? shell = mainWindow?.Page as Shell;

                    if (shell is null)
                    {
                        throw new InvalidOperationException("Main window doesn't use Shell");
                    }

                    await shell.GoToAsync(shellPreview.Route, NavigateAnimationsEnabled);
                }
                else if (previewUI is ContentPage contentPage)
                {
                    //MauiPreviewsApplication.Instance.Application.MainPage = contentPage;
                    await Application.Current!.MainPage!.Navigation.PushAsync(contentPage, NavigateAnimationsEnabled);
                }
            }
        });
    }
}
