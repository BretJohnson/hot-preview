using System.Threading.Tasks;
using StreamJsonRpc;

namespace HotPreview.SharedModel.Protocol;

public interface IPreviewAppService
{
    [JsonRpcMethod("components/list")]
    public Task<UIComponentInfo[]> GetComponentsAsync();

    [JsonRpcMethod("components/get")]
    public Task<UIComponentInfo?> GetComponentAsync(string componentName);

    [JsonRpcMethod("previews/navigate")]
    public Task NavigateToPreviewAsync(string componentName, string previewName);

    [JsonRpcMethod("previews/snapshot")]
    public Task<byte[]> GetPreviewSnapshotAsync(string componentName, string previewName);

    [JsonRpcMethod("commands/list")]
    public Task<PreviewCommandInfo[]> GetCommandsAsync();

    [JsonRpcMethod("commands/get")]
    public Task<PreviewCommandInfo?> GetCommandAsync(string commandName);

    [JsonRpcMethod("commands/invoke")]
    public Task InvokeCommandAsync(string commandName);
}
