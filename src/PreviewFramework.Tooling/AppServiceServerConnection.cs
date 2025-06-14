using System.Net.Sockets;
using StreamJsonRpc;

namespace PreviewFramework.Tooling;

public sealed class AppServiceServerConnection(AppServiceConnectionListener connectionListener, TcpClient tcpClient) :
    IPreviewAppControllerService
{
    private readonly AppServiceConnectionListener _connectionListener = connectionListener;
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

            _rpc = JsonRpc.Attach(connectionStream);
            _appService = _rpc.Attach<IPreviewAppService>();

            JsonRpc.Attach(connectionStream, this);

            // Register the connection with the listener
            _connectionListener.AddConnection(this);

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

    Task IPreviewAppControllerService.RegisterAppAsync(string projectPath, string platformName)
    {
        ProjectPath = projectPath;
        PlatformName = platformName;
        return Task.CompletedTask;
    }
}
