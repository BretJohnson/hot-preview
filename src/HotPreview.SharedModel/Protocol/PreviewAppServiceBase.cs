using System.Threading.Tasks;
using StreamJsonRpc;

namespace HotPreview.SharedModel.Protocol;

/// <summary>
/// Base class that declares JSON-RPC entrypoints for the preview app service.
/// Concrete implementations should override these methods; the JsonRpcMethod
/// attributes are inherited so implementers do not need to repeat them.
/// </summary>
public abstract class PreviewAppServiceBase : IPreviewAppService
{
    [JsonRpcMethod("appinfo/get")]
    public abstract Task<AppInfo> GetAppInfoAsync();

    [JsonRpcMethod("components/get")]
    public abstract Task<UIComponentInfo?> GetComponentAsync(string componentName);

    [JsonRpcMethod("previews/navigate")]
    public abstract Task NavigateToPreviewAsync(string componentName, string previewName);

    [JsonRpcMethod("previews/snapshot")]
    public abstract Task<byte[]> GetPreviewSnapshotAsync(string componentName, string previewName);

    [JsonRpcMethod("commands/get")]
    public abstract Task<CommandInfo?> GetCommandAsync(string commandName);

    [JsonRpcMethod("commands/invoke")]
    public abstract Task InvokeCommandAsync(string commandName);
}
