using System.Collections.Concurrent;

namespace PreviewFramework.Tooling;

/// <summary>
/// The AppManager is responsible for managing the connection(s) and metadata for a specific app. Apps
/// are uniquely identified by their startup project path. Each app connection is a separate instance
/// of the app that is running, perhaps on different platforms. Typically, there's only one app connection
/// but there can be multiple.
/// </summary>
/// <param name="appsManager">The apps manager that owns this app</param>
/// <param name="projectPath">The startup project path of the app</param>
public class AppManager(AppsManager appsManager, string projectPath)
{
    public AppsManager AppsManager { get; } = appsManager;

    public UIComponentsManager UIComponentsManager { get; set; }

    /// <summary>
    /// Whether the app shouldn't be automatically removed when there are no more app connections.
    /// This is false by default.
    /// </summary>
    public bool Pinned { get; set; } = false;

    private readonly ConcurrentDictionary<AppConnectionManager, AppConnectionManager> _appConnections = [];

    public string ProjectPath { get; } = projectPath;

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
}
