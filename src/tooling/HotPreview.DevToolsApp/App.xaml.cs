using HotPreview.DevToolsApp.ViewModels;
using HotPreview.Tooling.McpServer;
using Uno.Resizetizer;
using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace HotPreview.DevToolsApp;

public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        ApplicationView.PreferredLaunchViewSize = new Size(400, 800);

        var builder = this.CreateBuilder(args)
                // Add navigation support for toolkit controls such as TabBar and NavigationView
                .UseToolkitNavigation()
                .Configure(host => host
#if DEBUG
                    // Switch to Development environment when running in DEBUG
                    .UseEnvironment(Environments.Development)
#endif
                    .UseLogging(configure: (context, logBuilder) =>
                    {
                        // Configure log levels for different categories of logging
                        logBuilder
                            .SetMinimumLevel(
                                context.HostingEnvironment.IsDevelopment() ?
                                    LogLevel.Information :
                                    LogLevel.Warning)

                            // Default filters for core Uno Platform namespaces
                            .CoreLogLevel(LogLevel.Warning);

                        // Uno Platform namespace filter groups
                        // Uncomment individual methods to see more detailed logging
                        //// Generic Xaml events
                        //logBuilder.XamlLogLevel(LogLevel.Debug);
                        //// Layout specific messages
                        //logBuilder.XamlLayoutLogLevel(LogLevel.Debug);
                        //// Storage messages
                        //logBuilder.StorageLogLevel(LogLevel.Debug);
                        //// Binding related messages
                        //logBuilder.XamlBindingLogLevel(LogLevel.Debug);
                        //// Binder memory references tracking
                        //logBuilder.BinderMemoryReferenceLogLevel(LogLevel.Debug);
                        //// DevServer and HotReload related
                        //logBuilder.HotReloadCoreLogLevel(LogLevel.Information);
                        //// Debug JS interop
                        //logBuilder.WebAssemblyLogLevel(LogLevel.Debug);

                    }, enableUnoLogging: true)
                    .UseConfiguration(configure: configBuilder =>
                        configBuilder
                            .EmbeddedSource<App>()
                            .Section<AppConfig>()
                    )
                    .ConfigureServices((context, services) =>
                    {
                        // Register MCP HTTP server service
                        services.AddHostedService<McpHttpServerService>();
                    })
                    .UseNavigation(RegisterRoutes)
                );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        // Handle window closing to release single instance mutex
        MainWindow.Closed += (sender, args) =>
        {
            SingleInstanceManager.ReleaseMutex();
        };

        DevToolsManager.Initialize(SynchronizationContext.Current!);
        
        Host = await builder.NavigateAsync<Shell>();
        
        // Update DevToolsManager with MCP service after Host is created
        var mcpService = Host.Services.GetService<McpHttpServerService>();
        if (mcpService != null)
        {
            DevToolsManager.Instance.SetMcpService(mcpService);
        }

    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<MainPage, MainPageViewModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
                Nested:
                [
                    new ("Main", View: views.FindByViewModel<MainPageViewModel>(), IsDefault:true),
                ]
            )
        );
    }
}
