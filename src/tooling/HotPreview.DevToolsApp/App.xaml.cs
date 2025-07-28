using HotPreview.DevToolsApp.Services;
using HotPreview.DevToolsApp.ViewModels;
using HotPreview.Tooling;
using HotPreview.Tooling.McpServer;
using HotPreview.Tooling.Services;
using Serilog;
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

        IApplicationBuilder builder = this.CreateBuilder(args)
                // Add navigation support for toolkit controls such as TabBar and NavigationView
                .UseToolkitNavigation()
                .Configure(host => host
#if DEBUG
                    // Switch to Development environment when running in DEBUG
                    .UseEnvironment(Environments.Development)
#endif
                    .UseLogging(configure: (context, logBuilder) =>
                    {
                        // Configure simple file logging with daily rolling
                        string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                     "HotPreview", "Logs", "DevTools-.log");
                        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

                        Log.Logger = new LoggerConfiguration()
                            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                            .CreateLogger();

                        // Clear default providers (including console) and only use Serilog
                        logBuilder.ClearProviders();
                        logBuilder.AddSerilog(Log.Logger);

                        // Log application startup as the first message
                        Log.Information("Hot Preview DevTools application starting up...");

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
                        // Register UI context provider, capturing the UI SynchronizationContext as the ConfigureServices
                        // code runs on the UI thread so
                        services.AddSingleton(new UIContextProvider(SynchronizationContext.Current!));

                        // Register MCP HTTP server service, both as a singleton
                        // (so can pass to the DevToolsManager constructor) and as a hosted service
                        // (so it starts/stops automatically with the app lifecycle)
                        services.AddSingleton<McpHttpServerService>();
                        services.AddHostedService(provider =>
                            provider.GetService<McpHttpServerService>()!);

                        services.AddSingleton<StatusReporter>();

                        services.AddSingleton<DevToolsManager>();

                        // Register AppsManager from DevToolsManager for MCP server access
                        services.AddSingleton<AppsManager>(provider =>
                            provider.GetRequiredService<DevToolsManager>().AppsManager);
                    })
                    .UseNavigation(RegisterRoutes)
                );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        // Handle window closing to release single instance mutex and cleanup logging
        MainWindow.Closed += (sender, args) =>
        {
            Log.Information("Hot Preview DevTools application shutting down...");
            SingleInstanceManager.ReleaseMutex();
            Log.CloseAndFlush();
        };

        Host = await builder.NavigateAsync<Shell>();

        // Initialize DevToolsManager from DI container
        DevToolsManager devToolsManager = Host.Services.GetRequiredService<DevToolsManager>();
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
