using System.Threading.Tasks;
using StreamJsonRpc;

namespace HotPreview.SharedModel.Protocol;

public interface IPreviewAppService
{
    /// <summary>
    /// Gets all available UI components and their previews in the application.
    /// </summary>
    /// <returns>An array of UI component information objects.</returns>
    [JsonRpcMethod("components/list")]
    public Task<UIComponentInfo[]> GetComponentsAsync();

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
    /// Gets all available commands in the application. Commands trigger some action in the app, typically updating global state.
    /// Commands are global, not associated with a specific component.
    /// </summary>
    /// <returns>An array of commands.</returns>
    [JsonRpcMethod("commands/list")]
    public Task<PreviewCommandInfo[]> GetCommandsAsync();

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
