using System.Threading.Tasks;
using StreamJsonRpc;

namespace HotPreview.SharedModel.Protocol;

internal interface IPreviewAppControllerService
{
    [JsonRpcMethod("registerApp")]
    public Task RegisterAppAsync(string projectPath, string platformName);

    [JsonRpcMethod("notifications/components/listChanged")]
    public Task NotifyPreviewsChangedAsync();
}
