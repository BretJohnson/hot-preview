using System.Collections.Concurrent;
using System.Diagnostics;
using HotPreview.SharedModel;
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
                if (Settings.BringAppToFrontOnNavigate)
                {
                    TryBringAppWindowToForeground(appConnection);
                }
                // Fire and forget
                _ = appConnection.AppService?.NavigateToPreviewAsync(uiComponent.Name, preview.Name);
            }
        }
    }

    private static void TryBringAppWindowToForeground(AppConnectionManager appConnection)
    {
        try
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                // Minimal approach: use MainWindowHandle of the process if available.
                // TODO: Consider implementing the more sophisticated selection algorithm proposed
                // (filtering by visibility, not tool/owned windows, non-cloaked, ranking by size/Z-order, etc.)
                if (appConnection.DesktopAppProcessId is long pid && pid > 0)
                {
                    IntPtr hwnd = IntPtr.Zero;
                    try
                    {
                        using Process proc = Process.GetProcessById((int)pid);
                        hwnd = proc.MainWindowHandle;
                    }
                    catch
                    {
                        // Ignore lookup failures
                    }

                    if (hwnd != IntPtr.Zero)
                    {
                        if (WindowsNativeMethods.IsIconic(hwnd))
                        {
                            WindowsNativeMethods.ShowWindow(hwnd, WindowsNativeMethods.SW_RESTORE);
                        }
                        WindowsNativeMethods.SetForegroundWindow(hwnd);
                    }
                }
            }
            // TODO: macOS support: activate app/window using NSRunningApplication and AppKit APIs
        }
        catch
        {
            // Ignore failures in bringing the window to the foreground
        }
    }

    private static class WindowsNativeMethods
    {
        public const int SW_RESTORE = 9;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);
    }

    public async Task InvokeCommandAsync(CommandTooling command)
    {
        // Collect all connections that have the command
        List<Task> commandTasks = [];
        foreach (AppConnectionManager appConnection in AppConnections)
        {
            CommandTooling? existingCommand = appConnection.PreviewsManager?.GetCommand(command.Name);
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

    public async Task UpdateSnapshotsAsync(UIComponentTooling? uiComponent, PreviewTooling? preview)
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
            connectionTasks.Add(UpdateSnapshotsForConnectionAsync(appConnection, uiComponent, preview, snapshotsDirectory, totalPreviews));
        }

        await Task.WhenAll(connectionTasks);

        StatusReporter.UpdateStatus($"Successfully updated {totalPreviews} snapshot{(totalPreviews == 1 ? "" : "s")}");
    }

    public async Task UpdateCategorySnapshotsAsync(UIComponentCategory category)
    {
        if (PreviewsManager is null)
        {
            throw new InvalidOperationException("App has no UI components loaded");
        }

        StatusReporter.UpdateStatus("Preparing snapshot directory...");

        string projectDirectory = Path.GetDirectoryName(ProjectPath) ?? throw new InvalidOperationException($"Invalid project path: {ProjectPath}");
        string snapshotsDirectory = Path.Combine(projectDirectory, "snapshots");
        Directory.CreateDirectory(snapshotsDirectory);

        // Find components in the specified category using CategorizedUIComponents
        IEnumerable<UIComponentTooling> categoryComponents = PreviewsManager.CategorizedUIComponents
            .Where(categoryTuple => categoryTuple.Category.Equals(category))
            .SelectMany(categoryTuple => categoryTuple.UIComponents);

        // Count total previews to process for progress tracking
        int totalPreviews = CountPreviewsForComponents(categoryComponents);

        StatusReporter.UpdateStatus($"Updating {totalPreviews} snapshot{(totalPreviews == 1 ? "" : "s")} for category '{category.Name}'...");

        // Process each app connection in parallel
        List<Task> connectionTasks = [];
        foreach (AppConnectionManager appConnection in AppConnections)
        {
            connectionTasks.Add(UpdateCategorySnapshotsForConnectionAsync(appConnection, categoryComponents, snapshotsDirectory, totalPreviews));
        }

        await Task.WhenAll(connectionTasks);

        StatusReporter.UpdateStatus($"Successfully updated {totalPreviews} snapshot{(totalPreviews == 1 ? "" : "s")} for category '{category.Name}'");
    }

    private static int CountPreviewsForComponents(IEnumerable<UIComponentTooling> components)
    {
        int count = 0;
        foreach (UIComponentTooling component in components)
        {
            count += component.Previews.Count;
        }
        return count;
    }

    private async Task UpdateCategorySnapshotsForConnectionAsync(AppConnectionManager appConnection, IEnumerable<UIComponentTooling> categoryComponents, string snapshotsDirectory, int totalPreviews)
    {
        int currentProcessed = 0;

        foreach (UIComponentTooling currentUIComponent in categoryComponents)
        {
            foreach (PreviewTooling currentPreview in currentUIComponent.Previews)
            {
                if (appConnection.PreviewsManager?.HasPreview(currentUIComponent.Name, currentPreview.Name) ?? false)
                {
                    currentProcessed++;

                    string snapshotFileNameBase = PreviewsManager!.GetSnapshotFileNameBase(currentUIComponent, currentPreview);

                    StatusReporter.UpdateStatus($"Capturing snapshot {currentProcessed} of {totalPreviews}: {snapshotFileNameBase}");

                    var previewPair = new UIComponentPreviewPairTooling(currentUIComponent, currentPreview);
                    ImageSnapshot snapshot = await appConnection.GetPreviewSnapshotAsync(previewPair);

                    snapshot.Save(snapshotsDirectory, snapshotFileNameBase);
                }
            }
        }
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

    private async Task UpdateSnapshotsForConnectionAsync(AppConnectionManager appConnection, UIComponentTooling? uiComponent, PreviewTooling? preview, string snapshotsDirectory,
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

                    string snapshotFileNameBase = PreviewsManager!.GetSnapshotFileNameBase(currentUIComponent, currentPreview);

                    StatusReporter.UpdateStatus($"Capturing snapshot {currentProcessed} of {totalPreviews}: {snapshotFileNameBase}");

                    var previewPair = new UIComponentPreviewPairTooling(currentUIComponent, currentPreview);
                    ImageSnapshot snapshot = await appConnection.GetPreviewSnapshotAsync(previewPair);

                    snapshot.Save(snapshotsDirectory, snapshotFileNameBase);
                }
            }
        }
    }
}
