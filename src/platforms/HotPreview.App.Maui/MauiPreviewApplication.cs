using System;
using System.Collections.Generic;
using HotPreview;
using HotPreview.App.Maui;
using HotPreview.App.Maui.Pages;
using HotPreview.SharedModel.App;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

[assembly: PageUIComponentBaseType(MauiPreviewApplication.MauiPlatformType, "Microsoft.Maui.Controls.Page")]
[assembly: ControlUIComponentBaseType(MauiPreviewApplication.MauiPlatformType, "Microsoft.Maui.Controls.View")]

namespace HotPreview.App.Maui;

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
            () => new GetUIComponentsViaReflection(ServiceProvider, MainAssembly, AdditionalAppAssemblies,
                new MauiUIComponentExclusionFilter()).ToImmutable());

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

    public override string TransformConnectionStringForPlatform(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        // Split on the first ':' to separate IP list and port
        int colonIndex = connectionString.IndexOf(':');
        if (colonIndex < 0)
            return connectionString;

        string ipList = connectionString.Substring(0, colonIndex);
        string portPart = connectionString.Substring(colonIndex); // includes the colon
        var ips = new List<string>(ipList.Split(','));

        bool hasLoopback = ips.Contains("127.0.0.1");
        if (hasLoopback)
        {
            DevicePlatform platform = DeviceInfo.Platform;
            bool isVirtual = DeviceInfo.DeviceType == DeviceType.Virtual;

            // For desktop platforms, use loopback IP directly
            if (platform == DevicePlatform.WinUI || platform == DevicePlatform.MacCatalyst)
            {
                ips.Clear();
                ips.Add("127.0.0.1");
            }
            // For iOS simulator, the loopback address also works to connect to the host Mac
            else if (platform == DevicePlatform.iOS && isVirtual)
            {
                ips.Clear();
                ips.Add("127.0.0.1");
            }
            // For Android emulator, use the special loopback address for the host machine
            else if (platform == DevicePlatform.Android && isVirtual)
            {
                ips.Clear();
                ips.Add("10.0.2.2");
            }
            // Otherwise, use external IP(s)
            else
            {
                ips.RemoveAll(ip => ip == "127.0.0.1");
            }
        }

        return string.Join(",", ips) + portPart;
    }

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
