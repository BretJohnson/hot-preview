using System.Threading.Tasks;
using StreamJsonRpc;

namespace HotPreview.SharedModel.Protocol;

/// <summary>
/// Base class that declares JSON-RPC entrypoints for the tooling-side service.
/// Concrete implementations override these methods; attributes are inherited so
/// implementers do not need to repeat the JsonRpcMethod attributes.
/// </summary>
public abstract class PreviewAppToolingServiceBase : IPreviewAppToolingService
{
    [JsonRpcMethod("getToolingInfo")]
    public abstract Task<ToolingInfo> GetToolingInfoAsync();

    [JsonRpcMethod("registerApp")]
    public abstract Task RegisterAppAsync(string projectPath, string platformName, long? desktopAppProcessId);

    [JsonRpcMethod("notifications/components/listChanged")]
    public abstract Task NotifyComponentsChangedAsync();
}
