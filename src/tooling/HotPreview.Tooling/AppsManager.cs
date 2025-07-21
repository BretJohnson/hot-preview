using System.Collections.Concurrent;
using HotPreview.Tooling.Services;

namespace HotPreview.Tooling;

/// <summary>
/// The AppsManager is responsible for managing the apps that are connected to the tooling server.
/// </summary>
public class AppsManager(UIContextProvider uiContextProvider, StatusReporter statusReporter) : ToolingObservableObject(uiContextProvider.UIContext)
{
    private readonly ConcurrentDictionary<string, AppManager> _apps = [];

    /// <summary>
    /// Gets the current app(s), if any. A property change notification is raised when the apps collection changes.
    /// </summary>
    public IEnumerable<AppManager> Apps => _apps.Values;

    public int AppCount => _apps.Count;

    /// <summary>
    /// Gets the status reporter for updating application status messages.
    /// </summary>
    public StatusReporter StatusReporter { get; } = statusReporter;

    /// <summary>
    /// Gets or creates an AppManager for the specified project path.
    /// </summary>
    /// <param name="projectPath">The project path to get or create an AppManager for</param>
    /// <returns>The AppManager instance for the project path</returns>
    public AppManager GetOrCreateApp(string projectPath)
    {
        if (!_apps.TryGetValue(projectPath, out AppManager? appManager))
        {
            appManager = new AppManager(uiContextProvider, this, projectPath);
            _apps[projectPath] = appManager;

            OnPropertyChanged(nameof(Apps));
        }

        return appManager;
    }

    /// <summary>
    /// Removes an AppManager for the specified project path.
    /// </summary>
    /// <param name="projectPath">The project path to remove the AppManager for</param>
    /// <returns>True if the AppManager was successfully removed; otherwise, false</returns>
    public void RemoveApp(string projectPath)
    {
        if (_apps.TryRemove(projectPath, out _))
        {
            OnPropertyChanged(nameof(Apps));
        }
    }
}
