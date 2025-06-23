using System.Net.Sockets;
using StreamJsonRpc;
using PreviewFramework.SharedModel.Protocol;

namespace PreviewFramework.Tooling;

public sealed class AppConnectionManager(AppsManager appsManager, TcpClient tcpClient) :
    IPreviewAppControllerService
{
    private readonly AppsManager _appsManager = appsManager;
    private readonly TcpClient _tcpClient = tcpClient;
    private JsonRpc? _rpc;
    private IPreviewAppService? _appService;
    private AppManager? _appManager;

    public string? PlatformName { get; set; }
    public UIComponentsManagerTooling? UIComponentsManager { get; private set; }

    internal async Task HandleConnectionAsync()
    {
        try
        {
            NetworkStream connectionStream = _tcpClient.GetStream();

            _rpc = new JsonRpc(connectionStream, connectionStream);

            _rpc.AddLocalRpcTarget<IPreviewAppControllerService>(this, null);
            _appService = _rpc.Attach<IPreviewAppService>();

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

    public async Task RegisterAppAsync(string projectPath, string platformName)
    {
        if (_appManager is not null)
        {
            throw new InvalidOperationException($"App was already registered for this connection");
        }

        PlatformName = platformName;

        _appManager = _appsManager.GetOrCreateApp(projectPath);
        _appManager.AddAppConnection(this);

        UIComponentInfo[] uiComponentInfos = await _appService!.GetUIComponentsAsync();

        GetUIComponentsFromProtocol builder = new GetUIComponentsFromProtocol(uiComponentInfos);
        UIComponentsManager = builder.ToImmutable();
        // TODO: Implement NotifyUIComponentsChanged if needed
    }

    public Task NotifyUIComponentsChangedAsync() { return Task.CompletedTask; }
}
