using System;
using PreviewFramework;
using PreviewFramework.App.Maui;
using PreviewFramework.App.Maui.Pages;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

#if !MICROSOFT_PREVIEW_IN_TAP
[assembly: ExampleApplicationClass(typeof(MauiPreviewApplication))]

[assembly: PageUIComponentBaseType(MauiPreviewApplication.MauiPlatformType, "Microsoft.Maui.Controls.Page")]
[assembly: ControlUIComponentBaseType(MauiPreviewApplication.MauiPlatformType, "Microsoft.Maui.Controls.View")]
#endif

namespace PreviewFramework.App.Maui;

public partial class MauiPreviewApplication : PreviewApplication
{
    public static MauiPreviewApplication Instance => s_instance.Value;

    private static readonly Lazy<MauiPreviewApplication> s_instance =
        new(() =>
        {
            var instance = new MauiPreviewApplication();
            InitInstance(instance);
            return instance;
        });

    public const string MauiPlatformType = "MAUI";

    private readonly Lazy<UIComponentsManagerReflection> _uiComponentsManager;

    private MauiPreviewApplication()
    {
        // Use application default IServiceProvider, if available
        IElement? applicationElement = Application.Current;
        ServiceProvider = applicationElement?.Handler?.MauiContext?.Services;

        _uiComponentsManager = new Lazy<UIComponentsManagerReflection>(
            () => new UIComponentsManagerReflection(ServiceProvider, AdditionalAppAssemblies,
            new MauiUIComponentExclusionFilter()));

        PreviewAppService = new MauiPreviewAppService(this);

        PlatformName = DeviceInfo.Current.Platform.ToString();

#if WINDOWS
        AddKeyboardHandling();
#endif
    }

    public MauiPreviewAppService PreviewAppService { get; }

    public MauiPreviewNavigatorService PreviewNavigatorService { get; set; } = new MauiPreviewNavigatorService();

    public Window? PreviewUIWindow { get; private set; }

    public static void EnsureInitialized()
    {
        _ = Instance;
    }

    public override UIComponentsManagerReflection GetUIComponentsManager() => _uiComponentsManager.Value;

    public override PreviewAppService GetPreviewAppService() => PreviewAppService;

    public override string PlatformName { get; set; }

    public void AddPreviewUIShellItem(Shell shell, string title = "Previews", string? icon = null)
    {
        var previewsShellContent = new ShellContent
        {
            Title = title,
            Icon = icon,
            Route = "UIPreviews",
            ContentTemplate = new DataTemplate(typeof(PreviewsPage))
        };

        shell.Items.Add(previewsShellContent);
    }

    public void ShowPreviewUIWindow()
    {
        if (PreviewUIWindow is null)
        {
            PreviewUIWindow = new Window(new PreviewsPage());
            PreviewUIWindow.Title = "UI Previews";
            PreviewUIWindow.Destroying += PreviewUIWindow_Destroying;

            PreviewUIWindow.Width = 320;
            PreviewUIWindow.Height = 500;

            Window mainWindow = Application.Current!.Windows[0];
            if (mainWindow is not null)
            {
                // Position the window just left of the top left corner of the app window, but ensuring
                // it's fully on the screen
                PreviewUIWindow.X = double.Max(mainWindow.X - PreviewUIWindow.Width - 5, 0);
                PreviewUIWindow.Y = double.Max(mainWindow.Y, 0);
            }

            Application.Current?.OpenWindow(PreviewUIWindow);
        }
    }

    private void PreviewUIWindow_Destroying(object? sender, EventArgs e)
    {
        PreviewUIWindow = null;
    }
}
