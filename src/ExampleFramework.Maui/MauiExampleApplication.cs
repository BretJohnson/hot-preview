using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using ExampleFramework;
using ExampleFramework.App;
using ExampleFramework.Maui;
using ExampleFramework.Maui.Pages;

#if !MICROSOFT_PREVIEW_IN_TAP
[assembly: PreviewApplicationClass(typeof(MauiExampleApplication))]

[assembly: PageUIComponentBaseType(MauiExampleApplication.MauiPlatformType, "Microsoft.Maui.Controls.Page")]
[assembly: ControlUIComponentBaseType(MauiExampleApplication.MauiPlatformType, "Microsoft.Maui.Controls.View")]
#endif

namespace ExampleFramework.Maui;

public partial class MauiExampleApplication : ExampleApplication
{
    public static MauiExampleApplication Instance => s_instance.Value;

    private static readonly Lazy<MauiExampleApplication> s_instance =
        new(() =>
        {
            var instance = new MauiExampleApplication();
            InitInstance(instance);
            return instance;
        });

    public const string MauiPlatformType = "MAUI";

    private readonly Lazy<UIComponentsManagerReflection> _uiComponentsManager;

    private MauiExampleApplication()
    {
        // Use application default IServiceProvider, if available
        IElement? applicationElement = Application.Current;
        ServiceProvider = applicationElement?.Handler?.MauiContext?.Services;

        _uiComponentsManager = new Lazy<UIComponentsManagerReflection>(
            () => new UIComponentsManagerReflection(ServiceProvider, AdditionalAppAssemblies,
            new MauiUIComponentExclusionFilter()));

        PreviewAppService = new MauiPreviewAppService(this);

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

    public override ExampleAppService GetPreviewAppService() => PreviewAppService;

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
