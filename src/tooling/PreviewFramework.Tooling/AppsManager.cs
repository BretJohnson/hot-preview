namespace PreviewFramework.Tooling;

/// <summary>
/// The AppsManager is responsible for managing the apps that are connected to the tooling server.
/// </summary>
public class AppsManager
{
    private readonly Dictionary<string, AppManager> _apps = [];

    /// <summary>
    /// Gets the dictionary of AppManager instances, keyed by project path.
    /// </summary>
    public IReadOnlyDictionary<string, AppManager> Apps => _apps;

    /// <summary>
    /// Gets or creates an AppManager for the specified project path.
    /// </summary>
    /// <param name="projectPath">The project path to get or create an AppManager for</param>
    /// <returns>The AppManager instance for the project path</returns>
    public AppManager GetOrCreateApp(string projectPath)
    {
        if (!_apps.TryGetValue(projectPath, out AppManager? appManager))
        {
            appManager = new AppManager(this, projectPath);
            _apps[projectPath] = appManager;
        }

        return appManager;
    }

    /// <summary>
    /// Removes an AppManager for the specified project path.
    /// </summary>
    /// <param name="projectPath">The project path to remove the AppManager for</param>
    /// <returns>True if the AppManager was successfully removed; otherwise, false</returns>
    public bool RemoveApp(string projectPath)
    {
        return _apps.Remove(projectPath);
    }

    /// <summary>
    /// Clears all AppManagers.
    /// </summary>
    public void ClearApps()
    {
        _apps.Clear();
    }
}
