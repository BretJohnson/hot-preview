using System;
using System.IO;
using System.Threading.Tasks;
using HotPreview.SharedModel;
using HotPreview.SharedModel.App;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

namespace HotPreview.App.Maui;

public class MauiPreviewNavigatorService : IPreviewNavigator
{
    public bool NavigateAnimationsEnabled { get; set; } = false;

    public virtual void NavigateToPreview(UIComponentReflection uiComponent, PreviewReflection preview)
    {
        _ = NavigateToPreviewAsync(uiComponent, preview);
    }

    public virtual async Task NavigateToPreviewAsync(UIComponentReflection uiComponent, PreviewReflection preview)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            object? previewUI = preview.Create();

            if (uiComponent.Kind == UIComponentKind.Control)
            {
                var controlsPage = new ContentPage
                {
                    Content = (View)previewUI
                };

                await Application.Current!.MainPage!.Navigation.PushAsync(controlsPage, NavigateAnimationsEnabled);
            }
            else
            {
                if (previewUI is RoutePreview routePreview)
                {
                    Window? mainWindow = Application.Current!.Windows[0];

                    if (mainWindow?.Page is not Shell shell)
                    {
                        throw new InvalidOperationException("Main window doesn't use Shell");
                    }

                    await shell.GoToAsync(routePreview.Route, NavigateAnimationsEnabled);
                }
                else if (previewUI is ContentPage contentPage)
                {
                    //MauiPreviewApplication.Instance.Application.MainPage = contentPage;
                    await Application.Current!.MainPage!.Navigation.PushAsync(contentPage, NavigateAnimationsEnabled);
                }
            }
        });
    }

    public virtual async Task<byte[]> GetPreviewSnapshotAsync(UIComponentReflection uiComponent, PreviewReflection preview)
    {
        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            object? previewUI = preview.Create();

            if (uiComponent.Kind == UIComponentKind.Control)
            {
                return await CaptureViewAsPngAsync((View)previewUI);
            }
            else
            {
                if (previewUI is RoutePreview routePreview)
                {
                    Window? mainWindow = Application.Current!.Windows[0];

                    if (mainWindow?.Page is not Shell shell)
                    {
                        throw new InvalidOperationException("Main window doesn't use Shell");
                    }

                    await shell.GoToAsync(routePreview.Route, animate: false);
                    return await CaptureViewAsPngAsync(shell);
                }
                else if (previewUI is ContentPage contentPage)
                {
                    return await CaptureViewAsPngAsync(contentPage);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported preview UI type: {previewUI?.GetType()}");
                }
            }
        });
    }

    private static async Task<byte[]> CaptureViewAsPngAsync(IView view)
    {
        IScreenshotResult screenshot = await view.CaptureAsync() ??
            throw new InvalidOperationException("Failed to capture view as image");

        // Convert to PNG bytes using CopyToAsync
        using var memoryStream = new MemoryStream();
        await screenshot.CopyToAsync(memoryStream, ScreenshotFormat.Png);
        return memoryStream.ToArray();
    }
}
