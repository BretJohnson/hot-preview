using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using StreamJsonRpc;

namespace VisualTestUtils.AppConnector.Controller;

public class AppConnectorController : IDisposable
{
    private IAppService? _appService;
    private JsonRpc? _rpc;
    private readonly IPAddress _ipAddress;
    private readonly int _port;
    private bool _closed;
    private TcpListener? _listener;
    private TcpClient? _client;

    public ILogger? Logger { get; }

    public AppConnectorController(IPAddress ipAddress, int port, ILogger? logger = null)
    {
        _ipAddress = ipAddress;
        _port = port;
        Logger = logger;

        _listener = new TcpListener(_ipAddress, _port);
        _listener.Start();
    }

    public int Port =>
        _listener?.LocalEndpoint is IPEndPoint ipEndpoint ? ipEndpoint.Port : -1;

    public IAppService AppService => _appService!;

    /// <summary>
    /// Asynchronously accepts a client connection within the specified timeout period.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for a client connection.</param>
    /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation that, when completed, contains a TcpClient instance representing the accepted client connection.</returns>
    public Task WaitForConnectionAsync(TimeSpan timeout, CancellationToken token = default)
    {
        //this.Port = ((IPEndPoint)listener.LocalEndpoint).Port;
        //this.IP = ((IPEndPoint)listener.LocalEndpoint).Address.ToString();

        return AcceptAsync(timeout, token);

#if PREVIOUS
        // create a tcp listener on any available ip address and the specified port number
        var listener = new TcpListener(_ipAddress, _port);
        listener.Start();

        while (true)
        {
            // wait for incoming client requests
            TcpClient client = await listener.AcceptTcpClientAsync();

            // create a new JSON-RPC connection with the client stream
            _rpc = JsonRpc.Attach(client.GetStream());
            _appService = _rpc.Attach<IAppService>();

            // handle the connection asynchronously (fire and forget)
            _ = this.HandleConnectionAsync(client);
        }
#endif
    }

    /// <summary>
    /// Accepts an incoming TCP client connection.
    /// </summary>
    /// <param name="timespan">Timeout for allowing a connection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task AcceptAsync(TimeSpan timespan, CancellationToken cancellationToken)
    {
        if (!_closed)
        {
            // Link user cancellation token with timeout token.
            // If user cancels, return the task and assume they have handled it.
            // If timeout happens, throw since that means something went wrong and they need to handle it as an error.
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                cts.CancelAfter(timespan);
                using (cts.Token.Register(Close))
                {
                    try
                    {
                        // WithCancellationToken does not exist in net standard 2.0
                        if (_client is not null)
                            throw new InvalidOperationException("_client already set");

                        _client = await _listener!.AcceptTcpClientAsync();

                        // create a new JSON-RPC connection with the client stream
                        _rpc = JsonRpc.Attach(_client.GetStream());
                        _appService = _rpc.Attach<IAppService>();
                    }
                    catch (ObjectDisposedException)
                    {
                        if (!cts.IsCancellationRequested)
                        {
                            throw new TimeoutException($"Failed to initialize connection within {timespan}");
                        }
                    }
                    catch (SocketException) when (cts.IsCancellationRequested)
                    {
                        throw new TimeoutException($"Failed to initialize connection within {timespan}");
                    }
                    catch (SocketException) when (cancellationToken.IsCancellationRequested)
                    {
                        // If user has requested the cancellation, that should be fine, let it through.
                    }
                    catch (OperationCanceledException) when (!cts.IsCancellationRequested)
                    {
                        throw new TimeoutException($"Failed to initialize connection within {timespan}");
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        // If user has requested the cancellation, that should be fine, let it through.
                    }
                }
            }
        }
    }

    public bool IsConnected => _rpc is not null;

    /// <summary>
    /// Closes the TCP network stream.
    /// </summary>
    public void Close()
    {
        _closed = true;
        _rpc?.Dispose();
        _rpc = null;
        _client?.Close();
        _client = null;
        _listener?.Stop();
        _listener = null;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_closed && disposing)
        {
            Close();
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

#if PREVIOUS
    async Task HandleConnectionAsync(TcpClient client)
    {
        try
        {
            // handle JSON-RPC method calls and notifications on this connection
            await _rpc!.Completion;
        }
        catch (IOException)
        {
            // the client disconnected abruptly
        }
        catch (RemoteInvocationException e)
        {
            // there was an error in the JSON-RPC protocol
        }
        finally
        {
            // close the client gracefully
            client.Dispose();
        }
    }
#endif
}
