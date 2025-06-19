using System.Net.Sockets;
using StreamJsonRpc;

namespace PreviewFramework.Tooling;

public sealed class ToolingAppServerConnection(ToolingAppServerConnectionListener connectionListener, TcpClient tcpClient) :
    IPreviewAppControllerService
{
    private readonly ToolingAppServerConnectionListener _connectionListener = connectionListener;
    private readonly TcpClient _tcpClient = tcpClient;
    private JsonRpc? _rpc;
    private IPreviewAppService? _appService;

    public string? ProjectPath { get; set; }

    public string? PlatformName { get; set; }

    internal async Task HandleConnectionAsync()
    {
        try
        {
            NetworkStream connectionStream = _tcpClient.GetStream();

            _rpc = new JsonRpc(connectionStream, connectionStream);

            _rpc.AddLocalRpcTarget<IPreviewAppControllerService>(this, null);
            _appService = _rpc.Attach<IPreviewAppService>();

            _connectionListener.AddConnection(this);

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
            _connectionListener.RemoveConnection(this);

            _tcpClient.Dispose();
        }
    }

    public Task RegisterAppAsync(string projectPath, string platformName)
    {
        ProjectPath = projectPath;
        PlatformName = platformName;
        return Task.CompletedTask;
    }
}
