using System.Collections.Concurrent;

namespace HotPreview.Tooling;

/// <summary>
/// The AppManager is responsible for managing the connection(s) and metadata for a specific app. Apps
/// are uniquely identified by their startup project path. Each app connection is a separate instance
/// of the app that is running, perhaps on different platforms. Typically, there's only one app connection
/// but there can be multiple.
/// </summary>
/// <param name="appsManager">The apps manager that owns this app</param>
/// <param name="projectPath">The startup project path of the app</param>
public class AppManager(SynchronizationContext synchronizationContext, AppsManager appsManager, string projectPath) : ToolingObservableObject(synchronizationContext)
{
    private UIComponentsManagerTooling? _uiComponentsManager;

    public AppsManager AppsManager { get; } = appsManager;

    public string ProjectPath { get; } = projectPath;

    /// <summary>
    /// Gets or sets the UIComponentsManager for the app. A property change notification is raised when the UIComponentsManager collection changes.
    /// </summary>
    public UIComponentsManagerTooling? UIComponentsManager
    {
        get => _uiComponentsManager;
        set => SetProperty(ref _uiComponentsManager, value);
    }

    /// <summary>
    /// Whether the app shouldn't be automatically removed when there are no more app connections.
    /// This is false by default.
    /// </summary>
    public bool Pinned { get; set; } = false;

    private readonly ConcurrentDictionary<AppConnectionManager, AppConnectionManager> _appConnections = [];

    public string ProjectName => Path.GetFileNameWithoutExtension(ProjectPath);

    public IEnumerable<AppConnectionManager> AppConnections => _appConnections.Keys;

    /// <summary>
    /// Adds an AppConnectionManager for the app, failing if it already exists.
    /// </summary>
    /// <param name="appConnectionManager">The AppConnectionManager to add</param>
    public void AddAppConnection(AppConnectionManager appConnectionManager)
    {
        if (!_appConnections.TryAdd(appConnectionManager, appConnectionManager))
        {
            throw new InvalidOperationException($"AppConnectionManager for project '{ProjectPath}' is already registered.");
        }
    }

    /// <summary>
    /// Removes an AppConnectionManager from the app, if it exists.
    /// </summary>
    /// <param name="appConnectionManager">The AppConnectionManager to remove</param>
    public void RemoveAppConnection(AppConnectionManager appConnectionManager)
    {
        if (_appConnections.TryRemove(appConnectionManager, out _) && !Pinned && _appConnections.IsEmpty)
        {
            AppsManager.RemoveApp(ProjectPath);
        }
    }

    internal void UpdateUIComponents()
    {
        var uiComponentManagers = new List<UIComponentsManagerTooling>();
        foreach (AppConnectionManager appConnection in AppConnections)
        {
            if (appConnection.UIComponentsManager is not null)
            {
                uiComponentManagers.Add(appConnection.UIComponentsManager);
            }
        }

        if (uiComponentManagers.Count == 0)
        {
            UIComponentsManager = null;
        }
        else if (uiComponentManagers.Count == 1)
        {
            UIComponentsManager = uiComponentManagers[0];
        }
        else
        {
            UIComponentsManager = new GetUIComponentsConsolidated(uiComponentManagers).ToImmutable();
        }
    }

    public void NavigateToPreview(UIComponentTooling uiComponent, PreviewTooling preview)
    {
        foreach (AppConnectionManager appConnection in AppConnections)
        {
            if (appConnection.UIComponentsManager?.HasPreview(uiComponent.Name, preview.Name) ?? false)
            {
                // Fire and forget
                _ = appConnection.AppService?.NavigateToPreviewAsync(uiComponent.Name, preview.Name);
            }
        }
    }
}
