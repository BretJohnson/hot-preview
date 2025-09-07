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
    private bool _disposed;

    public async Task StartConnectionAsync(PreviewAppService appService)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ToolingAppClientConnection));

        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = _cancellationTokenSource.Token;

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
                        Debug.WriteLine("Hot Preview: Tooling reconnect failed; giving up.");
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

        // Query tooling info and validate protocol compatibility before registering
        ToolingInfo toolingInfo;
        try
        {
            toolingInfo = await _appToolingService.GetToolingInfoAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Debug.WriteLine("Hot Preview: error getting tooling info. Update the Hot Preview tooling if needed: dotnet tool update --global HotPreview.DevTools");
            throw new OperationCanceledException("Could not get Hot Preview protocol version", e);
        }

        if (Version.TryParse(ToolingInfo.CurrentProtocolVersion, out Version? appProtocol) &&
            Version.TryParse(toolingInfo.ProtocolVersion, out Version? toolingProtocol))
        {
            int majorCompare = toolingProtocol.Major.CompareTo(appProtocol.Major);
            if (majorCompare == 0)
            {
                int fullCompare = toolingProtocol.CompareTo(appProtocol);
                if (fullCompare > 0)
                {
                    Debug.WriteLine("Hot Preview: tooling is newer than the Hot Preview NuGet. Consider updating the NuGet.");
                }
                else if (fullCompare < 0)
                {
                    Debug.WriteLine("Hot Preview: tooling is older than the Hot Preview NuGet. Consider updating the Hot Preview tooling: 'dotnet tool update --global HotPreview.DevTools'.");
                }
            }
            else if (majorCompare > 0)
            {
                Debug.WriteLine("Hot Preview: tooling is newer and not compatible with this app. Update the app's Hot Preview NuGet.");
                throw new OperationCanceledException("Hot Preview protocol major version mismatch (tooling > app)");
            }
            else
            {
                Debug.WriteLine("Hot Preview: tooling is older and not compatible with this app. Update the Hot Preview tooling: 'dotnet tool update --global HotPreview.DevTools'.");
                throw new OperationCanceledException("Hot Preview protocol major version mismatch (tooling < app)");
            }
        }

        await _appToolingService.RegisterAppAsync(
            previewApplication.ProjectPath!,
            previewApplication.PlatformName,
            previewApplication.GetDesktopAppProcessId()).ConfigureAwait(false);
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

        // Retry every 2 seconds, giving up after 10 seconds
        if (totalSeconds < 10)
        {
            return 2000;
        }
        else
        {
            // Stop retrying
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
