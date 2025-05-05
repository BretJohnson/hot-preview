using System;
using System.Collections.Generic;

namespace Microsoft.UIPreview.App;

public abstract class PreviewApplication
{
    private static PreviewApplication? s_instance;

    private readonly List<string> _additionalAppAssemblies = [];

    public static PreviewApplication GetInstance() => s_instance ?? throw new InvalidOperationException($"{nameof(PreviewApplication)} not initialized");

    public abstract UIComponentsManagerReflection GetUIComponentsManager();

    public abstract PreviewAppService GetPreviewAppService();

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

    protected static void InitInstance(PreviewApplication instance)
    {
        s_instance = instance;
    }
}
