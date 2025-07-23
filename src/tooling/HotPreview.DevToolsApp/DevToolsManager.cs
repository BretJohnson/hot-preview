using System.Diagnostics;
using HotPreview.DevToolsApp.ViewModels;
using HotPreview.Tooling;
using HotPreview.Tooling.McpServer;
using HotPreview.Tooling.Services;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace HotPreview.DevToolsApp;

/// <summary>
/// Manager class for global application state for the HotPreview DevTools app.
/// </summary>
public partial class DevToolsManager : ObservableObject
{
    private readonly ILogger<DevToolsManager> _logger;
    private readonly ToolingAppServerConnectionListener _appServiceConnectionListener;
    private readonly McpHttpServerService _mcpHttpServerService;

    public AppsManager AppsManager { get; }

    public DevToolsManager(UIContextProvider uiContextProvider, StatusReporter statusReporter, McpHttpServerService mcpHttpServerService, ILogger<DevToolsManager> logger)
    {
        _logger = logger;
        _mcpHttpServerService = mcpHttpServerService;

        AppsManager = new(uiContextProvider.UIContext, statusReporter);

        // Initialize the app service connection listener
        _appServiceConnectionListener = new ToolingAppServerConnectionListener(AppsManager);
        _appServiceConnectionListener.StartListening();

        // Initial connection settings with MCP URL
        ConnectionSettingsJson.WriteSettings("devToolsConnectionSettings.json", _appServiceConnectionListener.Port, _mcpHttpServerService.ServerUrl);
    }

    public MainPageViewModel MainPageViewModel { get; set; } = null!;

    /// <summary>
    /// Gets or sets the current theme (light/dark).
    /// </summary>
    public string CurrentTheme { get; set; } = "Light";

    /// <summary>
    /// Updates the connection settings with MCP server URL if available.
    /// </summary>
    public void UpdateConnectionSettings()
    {
        ConnectionSettingsJson.WriteSettings("devToolsConnectionSettings.json", _appServiceConnectionListener.Port, _mcpHttpServerService.ServerUrl);
    }

    /// <summary>
    /// Runs the project by launching the csproj file.
    /// </summary>
    /// <returns>True if the process started successfully, false otherwise.</returns>
    public bool Run()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"ProjectPath\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            if (process == null)
            {
                _logger.LogError("Failed to start the process");
                return false;
            }

            _logger.LogInformation("Project started successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running the project");
            return false;
        }
    }

#if LATER
    /// <summary>
    /// Updates the project path and reloads the PreviewsManager.
    /// </summary>
    /// <param name="newProjectPath">The new project path to use.</param>
    /// <returns>True if the project was loaded successfully, false otherwise.</returns>
    public async Task<bool> UpdateProjectPathAsync(string newProjectPath)
    {
        if (string.IsNullOrWhiteSpace(newProjectPath))
        {
            _logger.LogError("Project path cannot be null or empty");
            return false;
        }

        if (!File.Exists(newProjectPath))
        {
            _logger.LogError("Project file does not exist: {ProjectPath}", newProjectPath);
            return false;
        }

        try
        {
            _logger.LogInformation("Updating project path to: {ProjectPath}", newProjectPath);

            // Update the project path
            ProjectPath = newProjectPath;

            // Reload the PreviewsManager with the new project
            _previewsManager = await CreatePreviewsManagerFromProjectAsync(newProjectPath);

            _logger.LogInformation("Project path updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project path to: {ProjectPath}", newProjectPath);
            return false;
        }
    }
#endif

    /// <summary>
    /// Creates a PreviewsManagerTooling from a single project file (.csproj) by loading and analyzing the project.
    /// </summary>
    /// <param name="projectPath">Path to the project file (.csproj)</param>
    /// <param name="includeApparentUIComponentsWithNoPreviews">Whether to include types that could be UI components but have no previews</param>
    /// <returns>A PreviewsManagerTooling instance with components from the project</returns>
    /// <exception cref="ArgumentException">Thrown when the project path is invalid</exception>
    /// <exception cref="FileNotFoundException">Thrown when the project file is not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when MSBuild cannot be located or project cannot be loaded</exception>
    public static async Task<PreviewsManagerTooling> CreatePreviewsManagerFromProjectAsync(string projectPath,
        bool includeApparentUIComponentsWithNoPreviews = false)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
            throw new ArgumentException("Project path cannot be null or empty", nameof(projectPath));

        if (!File.Exists(projectPath))
            throw new FileNotFoundException($"Project file not found: {projectPath}");

        EnsureMSBuildLocated();

        using MSBuildWorkspace workspace = MSBuildWorkspace.Create();

        try
        {
            Project project = await workspace.OpenProjectAsync(projectPath);
            Compilation compilation = await project.GetCompilationAsync() ??
                throw new InvalidOperationException($"Failed to get compilation for project: {projectPath}");

            return new GetUIComponentsFromRoslyn(compilation, includeApparentUIComponentsWithNoPreviews).ToImmutable();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load project '{projectPath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Ensures that MSBuild can be located for use with Roslyn workspaces.
    /// This method attempts to locate MSBuild and throws an exception if it cannot be found.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when MSBuild cannot be located</exception>
    public static void EnsureMSBuildLocated()
    {
        try
        {
            // Check if MSBuild is already registered
            if (!MSBuildLocator.IsRegistered)
            {
                // Try to register the default MSBuild instance
                VisualStudioInstance[] instances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
                if (instances.Length > 0)
                {
                    // Use the first available instance (usually the latest)
                    MSBuildLocator.RegisterInstance(instances.First());
                }
                else
                {
                    // Try to register the default .NET SDK MSBuild
                    MSBuildLocator.RegisterDefaults();
                }
            }

            // Try to create a workspace to verify MSBuild is available
            using var testWorkspace = MSBuildWorkspace.Create();
            // If we get here, MSBuild is available
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "MSBuild could not be located. Please ensure that either Visual Studio or the .NET SDK is installed. " +
                "For .NET SDK, make sure the Microsoft.Build.Locator package is properly configured if needed.", ex);
        }
    }
}
