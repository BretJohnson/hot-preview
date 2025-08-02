using System;
using System.IO;
using System.Threading;
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
                    Application.Current!.MainPage = contentPage;
                    //await Application.Current!.MainPage!.Navigation.PushAsync(contentPage, NavigateAnimationsEnabled);
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
                    Application.Current!.MainPage = contentPage;
                    await WaitForPageLoadedAsync(contentPage);
                    return await CaptureViewAsPngAsync(contentPage);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported preview UI type: {previewUI?.GetType()}");
                }
            }
        });
    }

    private static async Task WaitForPageLoadedAsync(ContentPage contentPage)
    {
        if (contentPage.IsLoaded)
        {
            // Page is already loaded, wait one frame to ensure rendering is complete
            await Task.Delay(16); // ~1 frame at 60fps
            return;
        }

        // Wait for the Loaded event
        var tcs = new TaskCompletionSource<bool>();

        void OnLoaded(object? sender, EventArgs e)
        {
            contentPage.Loaded -= OnLoaded;
            tcs.SetResult(true);
        }

        contentPage.Loaded += OnLoaded;

        // Add a timeout to prevent hanging indefinitely
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        cts.Token.Register(() =>
        {
            contentPage.Loaded -= OnLoaded;
            tcs.TrySetException(new TimeoutException("Page failed to load within 5 seconds"));
        });

        await tcs.Task;

        // Wait one additional frame to ensure rendering is complete
        await Task.Delay(16); // ~1 frame at 60fps
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
