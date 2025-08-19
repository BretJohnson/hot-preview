using System.Net.Sockets;
using HotPreview.SharedModel.Protocol;
using StreamJsonRpc;

namespace HotPreview.Tooling;

public sealed class AppConnectionManager(AppsManager appsManager, TcpClient tcpClient) :
    IPreviewAppControllerService
{
    private readonly AppsManager _appsManager = appsManager;
    private readonly TcpClient _tcpClient = tcpClient;
    private JsonRpc? _rpc;
    private AppManager? _appManager;

    public IPreviewAppService? AppService { get; private set; }

    public string? PlatformName { get; set; }
    public PreviewsManagerTooling? PreviewsManager { get; private set; }

    internal async Task HandleConnectionAsync()
    {
        try
        {
            NetworkStream connectionStream = _tcpClient.GetStream();

            _rpc = new JsonRpc(connectionStream, connectionStream);

            _rpc.AddLocalRpcTarget<IPreviewAppControllerService>(this, null);
            AppService = _rpc.Attach<IPreviewAppService>();

            try
            {
                _rpc.StartListening();
            }
            catch
            {
                _rpc.Dispose();
                throw;
            }

            // Handle JSON-RPC method calls and notifications on this connection
            await _rpc.Completion;
        }
        catch (IOException)
        {
            // The client disconnected abruptly
        }
        catch (RemoteInvocationException)
        {
            // There was an error in the JSON-RPC protocol
        }
        finally
        {
            _appManager?.RemoveAppConnection(this);
            _tcpClient.Dispose();
        }
    }

    [JsonRpcMethod("registerApp")]
    public async Task RegisterAppAsync(string projectPath, string platformName)
    {
        if (_appManager is not null)
        {
            throw new InvalidOperationException($"App was already registered for this connection");
        }

        PlatformName = platformName;

        _appManager = _appsManager.GetOrCreateApp(projectPath);
        _appManager.AddAppConnection(this);

        UIComponentInfo[] uiComponentInfos = await AppService!.GetComponentsAsync();
        PreviewCommandInfo[] previewCommandInfos = await AppService!.GetCommandsAsync();
        PreviewsManager = new GetPreviewsFromProtocol(uiComponentInfos, previewCommandInfos).ToImmutable();

        _appManager.UpdatePreviews();
    }

    [JsonRpcMethod("notifications/components/listChanged")]
    public async Task NotifyPreviewsChangedAsync()
    {
        UIComponentInfo[] uiComponentInfos = await AppService!.GetComponentsAsync();
        PreviewCommandInfo[] previewCommandInfos = await AppService!.GetCommandsAsync();
        PreviewsManager = new GetPreviewsFromProtocol(uiComponentInfos, previewCommandInfos).ToImmutable();

        _appManager?.UpdatePreviews();
    }

    public async Task<ImageSnapshot> GetPreviewSnapshotAsync(UIComponentPreviewPairTooling previewPair)
    {
        byte[] pngData = await AppService!.GetPreviewSnapshotAsync(previewPair.UIComponent.Name, previewPair.Preview.Name);
        return new ImageSnapshot(pngData, ImageSnapshotFormat.PNG);
    }
}
