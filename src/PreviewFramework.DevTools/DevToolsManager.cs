using System.Diagnostics;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using PreviewFramework.Tooling;

namespace PreviewFramework.DevTools;

/// <summary>
/// Singleton manager class for global application state for the PreviewFramework DevTools app.
/// </summary>
public class DevToolsManager
{
    private static readonly Lazy<DevToolsManager> s_instance = new(() => new DevToolsManager());
    private readonly ILogger<DevToolsManager> _logger;
    private IServiceProvider? _serviceProvider;
    private UIComponentsManager _uiComponentsManager;
    private string? _projectPath;
    private readonly ToolingAppServerConnectionListener _appServiceConnectionListener;

    /// <summary>
    /// Gets the singleton instance of the DevToolsManager.
    /// </summary>
    public static DevToolsManager Instance => s_instance.Value;

    /// <summary>
    /// Gets the UIComponentsManager instance.
    /// </summary>
    public UIComponentsManager UIComponentsManager => _uiComponentsManager;

    /// <summary>
    /// Get the current project path, if there is one.
    /// </summary>
    public string? ProjectPath => _projectPath;

    /// <summary>
    /// Private constructor to enforce singleton pattern.
    /// </summary>
    private DevToolsManager()
    {
        // Create a default logger if not provided through Initialize
        _logger = LoggerFactory.Create(builder => builder.AddDebug()).CreateLogger<DevToolsManager>();
        _logger.LogInformation("DevToolsManager instance created");

        // TODO: Don't hardcode this & initialize it asynchronously
        _projectPath = @"Q:\\src\\ui-preview-framework\\samples\\maui\\EcommerceMAUI\\EcommerceMAUI.csproj";
        _uiComponentsManager = CreateUIComponentsManagerFromProjectAsync(_projectPath).GetAwaiter().GetResult();

        // Initialize the app service connection listener
        _appServiceConnectionListener = new ToolingAppServerConnectionListener();
        _appServiceConnectionListener.StartListening();

        ConnectionSettingsJson.WriteSettings(_appServiceConnectionListener.Port);
    }

    /// <summary>
    /// Gets a value indicating whether the manager has been initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }

    public MainPageViewModel MainPageViewModel { get; set; } = null!;

    /// <summary>
    /// Gets the service provider for dependency resolution.
    /// </summary>
    public IServiceProvider ServiceProvider => _serviceProvider ??
        throw new InvalidOperationException("DevToolsManager has not been initialized. Call Initialize before using.");

    /// <summary>
    /// Gets or sets the current theme (light/dark).
    /// </summary>
    public string CurrentTheme { get; set; } = "Light";

    /// <summary>
    /// Initializes the DevToolsManager with services from the application.
    /// </summary>
    /// <param name="serviceProvider">The application's service provider.</param>
    public void Initialize(IServiceProvider serviceProvider)
    {
        if (IsInitialized)
        {
            _logger.LogWarning("DevToolsManager already initialized");
            return;
        }

        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        IsInitialized = true;

        // Get a better logger from DI
        ILoggerFactory? loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        if (loggerFactory is not null)
        {
            // Replace the default logger with one from DI
            ILogger<DevToolsManager> logger = loggerFactory.CreateLogger<DevToolsManager>();
            logger.LogInformation("DevToolsManager initialized with application services");
        }

        _logger.LogInformation("DevToolsManager initialized");
    }

    /// <summary>
    /// Gets a service of type T from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <returns>The service instance.</returns>
    public T GetService<T>() where T : notnull
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("DevToolsManager has not been initialized. Call Initialize before using.");
        }

        T? service = ServiceProvider.GetService<T>();
        if (service is null)
        {
            throw new InvalidOperationException($"Service of type {typeof(T).Name} could not be resolved.");
        }

        return service;
    }

    /// <summary>
    /// Tries to get a service of type T from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <param name="service">The output service instance if found.</param>
    /// <returns>True if the service was found, false otherwise.</returns>
    public bool TryGetService<T>(out T? service) where T : class
    {
        if (!IsInitialized)
        {
            service = null;
            return false;
        }

        service = ServiceProvider.GetService<T>();
        return service is not null;
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
                Arguments = $"run --project \"{_projectPath}\"",
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

    /// <summary>
    /// Updates the project path and reloads the UIComponentsManager.
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
            _projectPath = newProjectPath;

            // Reload the UIComponentsManager with the new project
            _uiComponentsManager = await CreateUIComponentsManagerFromProjectAsync(_projectPath);

            _logger.LogInformation("Project path updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project path to: {ProjectPath}", newProjectPath);
            return false;
        }
    }

    /// <summary>
    /// Creates a UIComponentsManager from a single project file (.csproj) by loading and analyzing the project.
    /// </summary>
    /// <param name="projectPath">Path to the project file (.csproj)</param>
    /// <param name="includeApparentUIComponentsWithNoPreviews">Whether to include types that could be UI components but have no previews</param>
    /// <returns>A UIComponentsManager instance with components from the project</returns>
    /// <exception cref="ArgumentException">Thrown when the project path is invalid</exception>
    /// <exception cref="FileNotFoundException">Thrown when the project file is not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when MSBuild cannot be located or project cannot be loaded</exception>
    public static async Task<UIComponentsManager> CreateUIComponentsManagerFromProjectAsync(string projectPath,
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

            return new UIComponentsManager(compilation, includeApparentUIComponentsWithNoPreviews);
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
