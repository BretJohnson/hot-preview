using System;
using System.Collections.Generic;

namespace PreviewFramework.App;

public abstract class PreviewApplication
{
    private static PreviewApplication? s_instance;

    private readonly List<string> _additionalAppAssemblies = [];
    private AppServiceClientConnection? _appServiceConnection;

    public static PreviewApplication GetInstance() => s_instance ??
        throw new InvalidOperationException($"{nameof(PreviewApplication)} not initialized");

    protected static void InitInstance(PreviewApplication instance)
    {
        s_instance = instance;
    }

    public abstract UIComponentsManagerReflection GetUIComponentsManager();

    public abstract PreviewAppService GetPreviewAppService();

    public void StartAppServiceConnection(string connectionString)
    {
        if (_appServiceConnection is not null)
        {
            throw new InvalidOperationException("AppServiceConnection is already initialized.");
        }

        _appServiceConnection = new AppServiceClientConnection(connectionString);

        // Fire and forget
        _ = _appServiceConnection.StartConnectionAsync(GetPreviewAppService()).ConfigureAwait(false);
    }

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
        if (serviceProvider == null)
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
