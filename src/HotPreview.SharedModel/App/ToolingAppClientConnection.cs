using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HotPreview.SharedModel.Protocol;

namespace HotPreview.SharedModel.App;

public sealed class ToolingAppClientConnection(string connectionString) : IDisposable
{
    private readonly string _connectionString = connectionString;
    private TcpClient? _tcpClient;
    private HotPreviewJsonRpc? _rpc;
    private IPreviewAppToolingService? _appToolingService;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _connectionTask;
    private bool _disposed;

    public async Task StartConnectionAsync(PreviewAppService appService)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ToolingAppClientConnection));

        _cancellationTokenSource = new CancellationTokenSource();
        _connectionTask = StartConnectionWithRetryAsync(appService, _cancellationTokenSource.Token);
        await _connectionTask.ConfigureAwait(false);
    }

    private async Task StartConnectionWithRetryAsync(PreviewAppService appService, CancellationToken cancellationToken)
    {
        // Parse _connectionString in the format "host:port"
        string[] parts = _connectionString.Split(':');
        if (parts.Length != 2)
            throw new FormatException($"Connection string '{_connectionString}' isn't in the format 'host:port'.");

        string host = parts[0];
        if (!int.TryParse(parts[1], out int port))
            throw new FormatException($"Connection string '{_connectionString}' port must be a valid integer.");

        // First connection attempt: do not retry
        try
        {
            await EstablishConnectionAsync(host, port, appService, cancellationToken).ConfigureAwait(false);
            Debug.WriteLine($"Hot Preview: Tooling connection established to {host}:{port}.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch
        {
            CleanupCurrentConnection();
            Debug.WriteLine($"Hot Preview: Couldn't connect to tooling on {host}:{port}");
            Debug.WriteLine($"Hot Preview: To use DevTools, run 'hot-preview' from a terminal (after 'dotnet tool install --global HotPreview.DevTools'), then relaunch the app.");
            return;
        }

        // Monitor connection, and on disconnection try to reconnect with retries
        while (!cancellationToken.IsCancellationRequested)
        {
            await MonitorConnectionAsync(cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
                break;

            // Disconnected: start reconnection loop with backoff and limited time
            Debug.WriteLine("Hot Preview: Tooling connection lost; attempting to reconnect...");
            CleanupCurrentConnection();

            DateTime reconnectStartTime = DateTime.UtcNow;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await EstablishConnectionAsync(host, port, appService, cancellationToken).ConfigureAwait(false);
                    Debug.WriteLine("Hot Preview: Tooling reconnection succeeded.");
                    break; // Reconnected
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch
                {
                    CleanupCurrentConnection();
                    TimeSpan elapsed = DateTime.UtcNow - reconnectStartTime;
                    int delayMs = CalculateRetryDelay(elapsed);
                    if (delayMs < 0)
                    {
                        // Give up silently per requirements.
                        return;
                    }
                    await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }

    private async Task EstablishConnectionAsync(string host, int port, PreviewAppService appService, CancellationToken cancellationToken)
    {
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(host, port).ConfigureAwait(false);
        NetworkStream networkStream = _tcpClient.GetStream();

        _rpc = new HotPreviewJsonRpc(networkStream, networkStream, appService);
        _appToolingService = _rpc.Attach<IPreviewAppToolingService>();

        PreviewApplication previewApplication = PreviewApplication.GetInstance();
        if (previewApplication.EnableJsonRpcTracing)
        {
            _rpc.TraceSource.Switch.Level = SourceLevels.All;
            _rpc.TraceSource.Listeners.Add(new DefaultTraceListener());
        }

        try
        {
            _rpc.StartListening();
        }
        catch
        {
            _rpc.Dispose();
            throw;
        }

        await _appToolingService.RegisterAppAsync(previewApplication.ProjectPath!,
            previewApplication.PlatformName).ConfigureAwait(false);
    }

    private async Task MonitorConnectionAsync(CancellationToken cancellationToken)
    {
        if (_rpc is null)
            return;

        try
        {
            // Wait for either cancellation or RPC completion (disconnection)
            await Task.WhenAny(
                _rpc.Completion,
                Task.Delay(Timeout.Infinite, cancellationToken)
            ).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected when cancellation is requested
        }
    }

    private static int CalculateRetryDelay(TimeSpan elapsed)
    {
        double totalSeconds = elapsed.TotalSeconds;

        if (totalSeconds < 5)
        {
            // Retry every 1 second for first 5 seconds
            return 1000;
        }
        else if (totalSeconds < 20)
        {
            // Retry every 2 seconds from 5-20 seconds
            return 2000;
        }
        else if (totalSeconds < 60)
        {
            // Retry every 4 seconds from 20-60 seconds
            return 4000;
        }
        else
        {
            // Stop retrying after 60 seconds
            return -1;
        }
    }

    private void CleanupCurrentConnection()
    {
        try
        {
            _rpc?.Dispose();
        }
        catch
        {
            // Ignore disposal exceptions
        }

        try
        {
            _tcpClient?.Close();
        }
        catch
        {
            // Ignore disposal exceptions
        }

        _rpc = null;
        _tcpClient = null;
        _appToolingService = null;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _cancellationTokenSource?.Cancel();
        CleanupCurrentConnection();
        _cancellationTokenSource?.Dispose();
    }
}
