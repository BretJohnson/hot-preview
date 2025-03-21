using Microsoft.Maui.Controls;
using System;
using Microsoft.Maui.ApplicationModel;

#if ANDROID
using Android.Runtime;
using Android.Views;
#endif

namespace Microsoft.UIPreview.Maui;

public static class PreviewExtensions
{
#if false
    public static MauiAppBuilder UsePreviewsOverlay(this MauiAppBuilder builder, Color? ribbonColor = null)
	{
		builder.ConfigureMauiHandlers(handlers =>
		{
			WindowHandler.Mapper.AppendToMapping("AddDebugOverlay", (handler, view) =>
            {
                Debug.WriteLine("Adding DebugOverlay");
                var overlay = new PreviewUIWindowOverlay(handler.VirtualView, ribbonColor);
                handler.VirtualView.AddOverlay(overlay);
            });
		});

		return builder;
	}
#endif

    public static void EnablePreviewUI(this Shell shell)
    {
        shell.Navigated += Shell_Navigated;
    }

    private static void Shell_Navigated(object? sender, ShellNavigatedEventArgs e)
    {
        // This fires when any page navigation completes
        Console.WriteLine($"Navigated to: {e.Current.Location}");

        // Get reference to the displayed page
        Shell.Current.CurrentPage.EnablePreviewUI();
    }

    public static void EnablePreviewUI(this Page page)
    {
        //EnablePreviewUI(page.Window);
        AddKeyListener(page);
    }

#if false
    public static void EnablePreviewUI(this Window window)
    {
        // Add the window overlay if it doesn't already exist
        if (!window.Overlays.Any(o => o is PreviewUIWindowOverlay))
        {
            var overlay = new PreviewUIWindowOverlay(window);
            window.AddOverlay(overlay);
        }
    }
#endif

    public static void AddKeyListener(this Page page)
    {
#if ANDROID
        Android.App.Activity? currentActivity = Platform.CurrentActivity;
#endif

#if false
        Microsoft.Maui.Handlers.PageHandler.Mapper.AppendToMapping("KeyboardListener", (handler, view) =>
        {
#if ANDROID
            if (handler.PlatformView is Android.Views.View platformView)
            {
                platformView.KeyPress += AndroidOnKeyPress;
            }
#endif
        });
#endif
    }

#if ANDROID
    private static void AndroidOnKeyPress(object? sender, Android.Views.View.KeyEventArgs e)
    {
        // Handle key event here
    }
#endif
}
