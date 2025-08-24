using System.Threading.Tasks;
using StreamJsonRpc;

namespace HotPreview.SharedModel.Protocol;

internal interface IPreviewAppToolingService
{
    /// <summary>
    /// Registers an application with the tooling service.
    /// </summary>
    /// <param name="projectPath">The file system path to the project.</param>
    /// <param name="platformName">The name of the platform (e.g., MAUI, WPF).</param>
    [JsonRpcMethod("registerApp")]
    public Task RegisterAppAsync(string projectPath, string platformName);

    /// <summary>
    /// Notifies the tooling service that the application information has changed,
    /// so it should requery to get the latest information.
    /// </summary>
    [JsonRpcMethod("notifications/appinfo/changed")]
    public Task NotifyAppInfoChangedAsync();
}
