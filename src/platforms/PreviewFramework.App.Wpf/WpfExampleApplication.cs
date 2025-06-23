using PreviewFramework;
using PreviewFramework.Model.App;
using PreviewFramework.App.Wpf;

[assembly: PageUIComponentBaseType(WpfPreviewApplication.WpfPlatformType, "System.Windows.Controls.Page")]
[assembly: PageUIComponentBaseType(WpfPreviewApplication.WpfPlatformType, "System.Windows.Window")]
[assembly: ControlUIComponentBaseType(WpfPreviewApplication.WpfPlatformType, "System.Windows.Media.Visual")]

namespace PreviewFramework.App.Wpf;

public class WpfPreviewApplication : PreviewApplication
{
    public static WpfPreviewApplication Instance => s_instance.Value;

    private static readonly Lazy<WpfPreviewApplication> s_instance =
        new(() =>
        {
            var instance = new WpfPreviewApplication();
            InitInstance(instance);
            return instance;
        });

    public const string WpfPlatformType = "WPF";

    private readonly Lazy<UIComponentsManagerReflection> _uiComponentsManager;

    private WpfPreviewApplication()
    {
        _uiComponentsManager = new Lazy<UIComponentsManagerReflection>(
            () => new GetUIComponentsViaReflection(ServiceProvider, AdditionalAppAssemblies, null).ToImmutable());

        PreviewAppService = new WpfPreviewAppService(this);
    }

    public WpfPreviewAppService PreviewAppService { get; }

    public WpfPreviewNavigatorService PreviewNavigatorService { get; set; } = new WpfPreviewNavigatorService();

    public override string PlatformName { get; set; } = "Windows";

    public static void EnsureInitialized()
    {
        _ = Instance;
    }

    public override UIComponentsManagerReflection GetUIComponentsManager() => _uiComponentsManager.Value;

    public override PreviewAppService GetPreviewAppService() => PreviewAppService;

}
