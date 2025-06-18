using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace PreviewFramework.Tooling;

public class ToolingAppServerConnectionListener : IDisposable
{
    public const int DefaultPort = 54242;

    private readonly TcpListener _listener;
    private readonly ConcurrentDictionary<ToolingAppServerConnection, ToolingAppServerConnection> _connections = [];

    public ToolingAppServerConnectionListener()
    {
        try
        {
            _listener = new TcpListener(IPAddress.Any, DefaultPort);
            _listener.Start();
        }
        catch (SocketException)
        {
            // If the default port is in use, fall back to an arbitrary port
            _listener = new TcpListener(IPAddress.Any, 0);
            _listener.Start();
        }
    }

    public void StartListening()
    {
        Task.Run(ListenLoopAsync);
    }

    internal void AddConnection(ToolingAppServerConnection connection)
    {
        if (!_connections.TryAdd(connection, connection))
        {
            throw new InvalidOperationException("Connection unexpected already added to listeners");
        }
    }

    internal void RemoveConnection(ToolingAppServerConnection connection)
    {
        _connections.TryRemove(connection, out _);
    }

    public int ConnectionCount => _connections.Count;

    private async Task ListenLoopAsync()
    {
        while (_listener is not null)
        {
            try
            {
                // Throws if cancellationToken is canceled before or during the wait
                TcpClient tcpClient = await _listener.AcceptTcpClientAsync();

                ToolingAppServerConnection appServiceConnection = new ToolingAppServerConnection(this, tcpClient);

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
