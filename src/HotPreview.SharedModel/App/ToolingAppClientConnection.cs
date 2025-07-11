using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using HotPreview.SharedModel.Protocol;
using StreamJsonRpc;

namespace HotPreview.SharedModel.App;

public class ToolingAppClientConnection(string connectionString)
{
    private readonly string _connectionString = connectionString;
    private TcpClient? _tcpClient;
    private JsonRpc? _rpc;
    private IPreviewAppControllerService? _appControllerService;

    public async Task StartConnectionAsync(PreviewAppService appService)
    {
        // Parse _connectionString in the format "host:port"
        string[] parts = _connectionString.Split(':');
        if (parts.Length != 2)
            throw new FormatException($"Connection string '{_connectionString}' isn't in the format 'host:port'.");

        string host = parts[0];
        if (!int.TryParse(parts[1], out int port))
            throw new FormatException($"Connection string '{_connectionString}' port must be a valid integer.");

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
}
