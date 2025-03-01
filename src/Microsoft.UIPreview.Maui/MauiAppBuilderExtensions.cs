using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.UIPreview.App;

namespace Microsoft.UIPreview.Maui;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder EnablePreviewMode<TApp>(this MauiAppBuilder builder) where TApp : class, IApplication
    {
#if true
        PreviewMode previewMode;
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            previewMode = PreviewMode.Gallery;
        else previewMode = PreviewMode.RemoteControl;
#else
        PreviewMode previewMode = GetPreviewMode(environmentVariable);
#endif

        if (previewMode == PreviewMode.None)
            return builder;

        if (previewMode == PreviewMode.Gallery)
        {
            // Remove the existing Application service, so we can replace it with ours
            builder.Services.RemoveAll<IApplication>();

            builder.UseMauiApp<App>();
        }
        else
        {
            //app.MainPage = new RemoteControlMainPage();
            //return true;
        }

        return builder;
    }

#if OLD
    public static bool LaunchInPreviewMode(string? galleryAppTitle, string? environmentVariable)
    {
#if true
        PreviewMode previewMode;
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            previewMode = PreviewMode.Gallery;
        else previewMode = PreviewMode.RemoteControl;
#else
        PreviewMode previewMode = GetPreviewMode(environmentVariable);
#endif

        if (previewMode == PreviewMode.None)
            return false;

        if (previewMode == PreviewMode.Gallery)
        {
            app.MainPage = new DesktopMainPage();

            IReadOnlyList<Window> appWindows = app.Windows;
            if (appWindows.Count > 0)
                appWindows[0].Title = galleryAppTitle;

            return true;
        }
        else
        {
            app.MainPage = new RemoteControlMainPage();
            return true;
        }
    }
#endif

    public static PreviewMode GetPreviewMode(string? environmentVariable = null)
    {
        if (environmentVariable != null)
        {
            string? value = Environment.GetEnvironmentVariable(environmentVariable);
            if (value != null && !string.Equals(value, "0") && !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
                return PreviewMode.Gallery;
        }

        string[] args = Environment.GetCommandLineArgs();

        int count = args.Length;
        for (int i = 0; i < count; i++)
        {
            string arg = args[i];
            if (string.Equals(arg, "--previews-gallery", StringComparison.OrdinalIgnoreCase))
            {
                return PreviewMode.Gallery;
            }
            else if (string.Equals(arg, "--previews-remote-control", StringComparison.OrdinalIgnoreCase))
            {
                return PreviewMode.RemoteControl;
            }
        }

        return PreviewMode.None;
    }
}
