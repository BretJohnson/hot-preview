using System.Collections.Concurrent;
using HotPreview.Tooling.Services;

namespace HotPreview.Tooling;

/// <summary>
/// The AppManager is responsible for managing the connection(s) and metadata for a specific app. Apps
/// are uniquely identified by their startup project path. Each app connection is a separate instance
/// of the app that is running, perhaps on different platforms. Typically, there's only one app connection
/// but there can be multiple.
/// </summary>
/// <param name="appsManager">The apps manager that owns this app</param>
/// <param name="projectPath">The startup project path of the app</param>
public class AppManager(AppsManager appsManager, string projectPath) :
    ToolingObservableObject(appsManager.SynchronizationContext)
{
    private PreviewsManagerTooling? _previewsManager;

    public AppsManager AppsManager { get; } = appsManager;

    public string ProjectPath { get; } = projectPath;

    public StatusReporter StatusReporter { get; } = appsManager.StatusReporter;

    /// <summary>
    /// Gets or sets the PreviewsManager for the app. A property change notification is raised when the PreviewsManager collection changes.
    /// </summary>
    public PreviewsManagerTooling? PreviewsManager
    {
        get => _previewsManager;
        set => SetProperty(ref _previewsManager, value);
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

    internal void UpdatePreviews()
    {
        var previewsManagers = new List<PreviewsManagerTooling>();
        foreach (AppConnectionManager appConnection in AppConnections)
        {
            if (appConnection.PreviewsManager is not null)
            {
                previewsManagers.Add(appConnection.PreviewsManager);
            }
        }

        if (previewsManagers.Count == 0)
        {
            PreviewsManager = null;
        }
        else if (previewsManagers.Count == 1)
        {
            PreviewsManager = previewsManagers[0];
        }
        else
        {
            PreviewsManager = new GetPreviewsConsolidated(previewsManagers).ToImmutable();
        }
    }

    public void NavigateToPreview(UIComponentTooling uiComponent, PreviewTooling preview)
    {
        foreach (AppConnectionManager appConnection in AppConnections)
        {
            if (appConnection.PreviewsManager?.HasPreview(uiComponent.Name, preview.Name) ?? false)
            {
                // Fire and forget
                _ = appConnection.AppService?.NavigateToPreviewAsync(uiComponent.Name, preview.Name);
            }
        }
    }

    public async Task InvokeCommandAsync(PreviewCommandTooling command)
    {
        // Collect all connections that have the command
        List<Task> commandTasks = [];
        foreach (AppConnectionManager appConnection in AppConnections)
        {
            PreviewCommandTooling? existingCommand = appConnection.PreviewsManager?.GetCommand(command.Name);
            if (existingCommand is not null && appConnection.AppService is not null)
            {
                commandTasks.Add(appConnection.AppService.InvokeCommandAsync(command.Name));
            }
        }

        if (commandTasks.Count == 0)
        {
            throw new InvalidOperationException($"Command '{command.Name}' not found in any connected app");
        }

        // Execute all commands in parallel
        await Task.WhenAll(commandTasks);
    }

    public async Task UpdatePreviewSnapshotsAsync(UIComponentTooling? uiComponent, PreviewTooling? preview)
    {
        if (uiComponent is null && preview is not null)
        {
            throw new InvalidOperationException("Cannot specify a preview without specifying a UI component");
        }

        if (PreviewsManager is null)
        {
            throw new InvalidOperationException("App has no UI components loaded");
        }

        StatusReporter.UpdateStatus("Preparing snapshot directory...");

        string projectDirectory = Path.GetDirectoryName(ProjectPath) ?? throw new InvalidOperationException($"Invalid project path: {ProjectPath}");
        string snapshotsDirectory = Path.Combine(projectDirectory, "snapshots");
        Directory.CreateDirectory(snapshotsDirectory);

        // Count total previews to process for progress tracking
        int totalPreviews = CountPreviewsToProcess(uiComponent, preview);

        StatusReporter.UpdateStatus($"Updating {totalPreviews} snapshot{(totalPreviews == 1 ? "" : "s")}...");

        // Process each app connection in parallel
        List<Task> connectionTasks = [];
        foreach (AppConnectionManager appConnection in AppConnections)
        {
            connectionTasks.Add(UpdatePreviewSnapshotsForConnectionAsync(appConnection, uiComponent, preview, snapshotsDirectory, totalPreviews));
        }

        await Task.WhenAll(connectionTasks);

        StatusReporter.UpdateStatus($"Successfully updated {totalPreviews} snapshot{(totalPreviews == 1 ? "" : "s")}");
    }

    private int CountPreviewsToProcess(UIComponentTooling? uiComponent, PreviewTooling? preview)
    {
        if (PreviewsManager is null)
            return 0;

        IEnumerable<UIComponentTooling> componentsToProcess = uiComponent is not null ?
            [uiComponent] : PreviewsManager.UIComponents;

        int count = 0;
        foreach (UIComponentTooling currentUIComponent in componentsToProcess)
        {
            IReadOnlyList<PreviewTooling> previewsToProcess = preview is not null
                ? [preview]
                : currentUIComponent.Previews;

            count += previewsToProcess.Count;
        }

        return count;
    }

    private async Task UpdatePreviewSnapshotsForConnectionAsync(AppConnectionManager appConnection, UIComponentTooling? uiComponent, PreviewTooling? preview, string snapshotsDirectory,
        int totalPreviews)
    {
        IEnumerable<UIComponentTooling> componentsToProcess = uiComponent is not null ?
            [uiComponent] : PreviewsManager!.UIComponents;

        int currentProcessed = 0;

        foreach (UIComponentTooling currentUIComponent in componentsToProcess)
        {
            IReadOnlyList<PreviewTooling> previewsToProcess = preview is not null
                ? [preview]
                : currentUIComponent.Previews;

            foreach (PreviewTooling currentPreview in previewsToProcess)
            {
                if (appConnection.PreviewsManager?.HasPreview(currentUIComponent.Name, currentPreview.Name) ?? false)
                {
                    currentProcessed++;

                    string previewDisplayName;
                    if (uiComponent is null)
                    {
                        previewDisplayName = !currentUIComponent.HasMultiplePreviews
                            ? currentUIComponent.DisplayName
                            : $"{currentUIComponent.DisplayName} - {currentPreview.DisplayName}";
                    }
                    else
                    {
                        // If we're only processing a single UI component, then don't include the component name in the display name
                        previewDisplayName = currentPreview.DisplayName;
                    }

                    StatusReporter.UpdateStatus($"Capturing snapshot {currentProcessed} of {totalPreviews}: {previewDisplayName}");

                    var previewPair = new UIComponentPreviewPairTooling(currentUIComponent, currentPreview);
                    ImageSnapshot snapshot = await appConnection.GetPreviewSnapshotAsync(previewPair);

                    string componentShortName = PreviewsManager!.GetUIComponentShortName(currentUIComponent.Name);
                    string fileNameBase = !currentUIComponent.HasMultiplePreviews ? componentShortName : $"{componentShortName}-{currentPreview.Name}";
                    snapshot.Save(snapshotsDirectory, fileNameBase);
                }
            }
        }
    }
}
