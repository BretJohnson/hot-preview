using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HotPreview.SharedModel.Protocol;
using StreamJsonRpc;

namespace HotPreview.SharedModel.App;

public sealed class ToolingAppClientConnection(string connectionString) : IDisposable
{
    private readonly string _connectionString = connectionString;
    private TcpClient? _tcpClient;
    private JsonRpc? _rpc;
    private IPreviewAppControllerService? _appControllerService;
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

        DateTime connectionStartTime = DateTime.UtcNow;
        int attemptCount = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                attemptCount++;
                await EstablishConnectionAsync(host, port, appService, cancellationToken).ConfigureAwait(false);

                // If we get here, connection was successful. Monitor for disconnection.
                await MonitorConnectionAsync(cancellationToken).ConfigureAwait(false);

                // Connection lost, prepare for reconnection
                CleanupCurrentConnection();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                CleanupCurrentConnection();

                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            // Calculate delay based on elapsed time since first connection attempt
            TimeSpan elapsed = DateTime.UtcNow - connectionStartTime;
            int delayMs = CalculateRetryDelay(elapsed);

            if (delayMs < 0)
            {
                break;
            }

            await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task EstablishConnectionAsync(string host, int port, PreviewAppService appService, CancellationToken cancellationToken)
    {
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(host, port).ConfigureAwait(false);
        NetworkStream networkStream = _tcpClient.GetStream();

        _rpc = new JsonRpc(networkStream, networkStream, appService);
        _appControllerService = _rpc.Attach<IPreviewAppControllerService>();

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

        await _appControllerService.RegisterAppAsync(previewApplication.ProjectPath!,
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
        _appControllerService = null;
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
