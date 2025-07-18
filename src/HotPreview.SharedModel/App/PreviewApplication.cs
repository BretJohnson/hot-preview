using System;
using System.Collections.Generic;
using System.Reflection;

namespace HotPreview.SharedModel.App;

public abstract class PreviewApplication
{
    private static PreviewApplication? s_instance;

    private readonly List<string> _additionalAppAssemblies = [];
    private ToolingAppClientConnection? _toolingConnection;

    public static PreviewApplication GetInstance() => s_instance ??
        throw new InvalidOperationException($"{nameof(PreviewApplication)} not initialized");

    protected static void InitInstance(PreviewApplication instance)
    {
        s_instance = instance;
    }

    public abstract UIComponentsManagerReflection GetUIComponentsManager();

    public abstract PreviewAppService GetPreviewAppService();

    public abstract IPreviewNavigator GetPreviewNavigator();

    public void StartToolingConnection()
    {
        if (_toolingConnection is not null)
        {
            throw new InvalidOperationException("AppServiceConnection is already initialized.");
        }

        if (ToolingConnectionString is null)
        {
            throw new InvalidOperationException("ToolingConnectionString is not set.");
        }

        string platformConnectionString = TransformConnectionStringForPlatform(ToolingConnectionString);
        _toolingConnection = new ToolingAppClientConnection(platformConnectionString);

        // Fire and forget
        _ = _toolingConnection.StartConnectionAsync(GetPreviewAppService()).ConfigureAwait(false);
    }

    public string? ToolingConnectionString { get; set; }

    public string? ProjectPath { get; set; }

    public abstract string PlatformName { get; set; }

    /// <summary>
    /// Gets or sets whether JsonRpc diagnostic tracing is enabled.
    /// When true, JsonRpc communication will be traced to the console.
    /// </summary>
    public bool EnableJsonRpcTracing { get; set; } = false;

    public Assembly? MainAssembly { get; set; }

    /// <summary>
    /// The app's service provider, which when present can be used to instantiate
    /// UI components via dependency injection.
    /// </summary>
    public IServiceProvider? ServiceProvider { get; set; } = null;

    public TService GetRequiredService<TService>() where TService : class
    {
        IServiceProvider? serviceProvider = ServiceProvider;
        if (serviceProvider is null)
        {
            throw new InvalidOperationException("ServiceProvider is not available.");
        }

        object service = serviceProvider.GetService(typeof(TService)) ??
            throw new InvalidOperationException($"Service of type {typeof(TService).FullName} is not registered.");

        return (TService)service;
    }

    public void AddAdditionalAppAssembly(string assemblyName)
    {
        _additionalAppAssemblies.Add(assemblyName);
    }

    public IEnumerable<string> AdditionalAppAssemblies => _additionalAppAssemblies;

    /// <summary>
    /// Transforms a tooling connection string to be appropriate for the current platform.
    /// By default, returns the input string unchanged. Platform-specific overrides can adjust
    /// the connection string (such as IP addresses or ports) to match the requirements or conventions
    /// of the target platform or device.
    /// </summary>
    public virtual string TransformConnectionStringForPlatform(string connectionString)
    {
        // By default, return the string supplied
        return connectionString;
    }
}
