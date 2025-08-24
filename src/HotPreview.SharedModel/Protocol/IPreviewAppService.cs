using System.Threading.Tasks;
using StreamJsonRpc;

namespace HotPreview.SharedModel.Protocol;

public interface IPreviewAppService
{
    /// <summary>
    /// Gets the application information containing all available UI components and preview commands in the application.
    /// </summary>
    /// <returns>An AppInfo object containing components and commands.</returns>
    [JsonRpcMethod("appinfo/get")]
    public Task<AppInfo> GetAppInfoAsync();

    /// <summary>
    /// Gets information about a specific UI component by name.
    /// </summary>
    /// <param name="componentName">The name of the component to retrieve.</param>
    /// <returns>The UI component information, or null if not found.</returns>
    [JsonRpcMethod("components/get")]
    public Task<UIComponentInfo?> GetComponentAsync(string componentName);

    /// <summary>
    /// Navigates to a specific preview for a UI component in the app.
    /// </summary>
    /// <param name="componentName">The name of the component containing the preview.</param>
    /// <param name="previewName">The name of the preview to navigate to.</param>
    [JsonRpcMethod("previews/navigate")]
    public Task NavigateToPreviewAsync(string componentName, string previewName);

    /// <summary>
    /// Captures a visual snapshot for a specific preview.
    /// </summary>
    /// <param name="componentName">The name of the component containing the preview.</param>
    /// <param name="previewName">The name of the preview to capture.</param>
    /// <returns>A byte array containing the snapshot image data.</returns>
    [JsonRpcMethod("previews/snapshot")]
    public Task<byte[]> GetPreviewSnapshotAsync(string componentName, string previewName);

    /// <summary>
    /// Gets information about a specific command by name.
    /// </summary>
    /// <param name="commandName">The name of the command to retrieve.</param>
    /// <returns>The command information, or null if not found.</returns>
    [JsonRpcMethod("commands/get")]
    public Task<PreviewCommandInfo?> GetCommandAsync(string commandName);

    /// <summary>
    /// Invokes a command by name.
    /// </summary>
    /// <param name="commandName">The name of the command to invoke.</param>
    /// <exception cref="ArgumentException">Thrown when the command is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the command execution fails.</exception>
    [JsonRpcMethod("commands/invoke")]
    public Task InvokeCommandAsync(string commandName);
}
