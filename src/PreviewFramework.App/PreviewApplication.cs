using System;
using System.Collections.Generic;

namespace PreviewFramework.App;

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

        _toolingConnection = new ToolingAppClientConnection(ToolingConnectionString);

        // Fire and forget
        _ = _toolingConnection.StartConnectionAsync(GetPreviewAppService()).ConfigureAwait(false);
    }

    public string? ToolingConnectionString { get; set; }

    public string? ProjectPath { get; set; }

    public abstract string PlatformName { get; set; }

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
}
