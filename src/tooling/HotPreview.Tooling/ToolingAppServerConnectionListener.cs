using System.Net;
using System.Net.Sockets;

namespace HotPreview.Tooling;

public class ToolingAppServerConnectionListener : IDisposable
{
    public const int DefaultPort = 54242;

    private readonly AppsManager _appsManager;
    private readonly TcpListener _listener;

    public ToolingAppServerConnectionListener(AppsManager appsManager)
    {
        _appsManager = appsManager;
        try
        {
            _listener = new TcpListener(IPAddress.Any, DefaultPort);
            _listener.Start();
        }
        catch (SocketException)
        {
            // If the default port is in use, do not fall back.
            // A fixed port is required for discovery by build tooling.
            throw;
        }
    }

    public void StartListening()
    {
        Task.Run(ListenLoopAsync);
    }

    private async Task ListenLoopAsync()
    {
        while (_listener is not null)
        {
            try
            {
                // Throws if cancellationToken is canceled before or during the wait
                TcpClient tcpClient = await _listener.AcceptTcpClientAsync();

                var appServiceConnection = new AppConnectionManager(_appsManager, tcpClient);

                // Handle each client connection asynchronously (fire and forget)
                _ = appServiceConnection.HandleConnectionAsync();
            }
            catch (ObjectDisposedException)
            {
                // Listener was stopped, exit loop
                break;
            }
            catch (OperationCanceledException)
            {
                // Cancellation requested, exit loop
                break;
            }
            catch (Exception)
            {
                // Log or handle error as needed
            }
        }
    }

    public int Port =>
        _listener?.LocalEndpoint is IPEndPoint ipEndpoint ? ipEndpoint.Port : -1;

    public void Dispose()
    {
        _listener.Stop();

        GC.SuppressFinalize(this);
    }
}
