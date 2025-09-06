using System.Net;
using System.Net.Sockets;
using HotPreview.SharedModel.Protocol;

namespace HotPreview.Tooling;

public sealed class AppConnectionManager(AppsManager appsManager, TcpClient tcpClient) :
    PreviewAppToolingServiceBase
{
    private readonly AppsManager _appsManager = appsManager;
    private readonly TcpClient _tcpClient = tcpClient;
    private HotPreviewJsonRpc? _rpc;
    private AppManager? _appManager;

    public IPreviewAppService? AppService { get; private set; }

    public string? PlatformName { get; set; }
    public PreviewsManagerTooling? PreviewsManager { get; private set; }

    internal async Task HandleConnectionAsync()
    {
        try
        {
            NetworkStream connectionStream = _tcpClient.GetStream();

            _rpc = new HotPreviewJsonRpc(connectionStream, connectionStream);

            _rpc.AddLocalRpcTarget<IPreviewAppToolingService>(this);
            AppService = _rpc.Attach<IPreviewAppService>();

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
        catch (StreamJsonRpc.RemoteInvocationException)
        {
            // There was an error in the JSON-RPC protocol
        }
        finally
        {
            _appManager?.RemoveAppConnection(this);
            _tcpClient.Dispose();
        }
    }

    public override async Task RegisterAppAsync(string projectPath, string platformName)
    {
        if (_appManager is not null)
        {
            throw new InvalidOperationException($"App was already registered for this connection");
        }

        PlatformName = platformName;

        _appManager = _appsManager.GetOrCreateApp(projectPath);
        _appManager.AddAppConnection(this);

        AppInfo appInfo = await AppService!.GetAppInfoAsync();
        PreviewsManager = new GetPreviewsFromProtocol(appInfo).ToImmutable();

        _appManager.UpdatePreviews();
    }

    public override async Task NotifyComponentsChangedAsync()
    {
        AppInfo appInfo = await AppService!.GetAppInfoAsync();
        PreviewsManager = new GetPreviewsFromProtocol(appInfo).ToImmutable();

        _appManager?.UpdatePreviews();
    }

    public async Task<ImageSnapshot> GetPreviewSnapshotAsync(UIComponentPreviewPairTooling previewPair)
    {
        byte[] pngData = await AppService!.GetPreviewSnapshotAsync(previewPair.UIComponent.Name, previewPair.Preview.Name);
        return new ImageSnapshot(pngData, ImageSnapshotFormat.PNG);
    }

    public override Task<ToolingInfo> GetToolingInfoAsync()
    {
        int listenerPort = -1;
        try
        {
            if (_tcpClient.Client.LocalEndPoint is IPEndPoint ep)
            {
                listenerPort = ep.Port;
            }
        }
        catch
        {
        }

        if (listenerPort <= 0)
        {
            throw new InvalidOperationException("DevTools listener port could not be determined from the accepted connection.");
        }

        string connectionString = BuildConnectionString(listenerPort);

        ToolingInfo info = new(
            ToolingInfo.CurrentProtocolVersion,
            connectionString,
            null // Optional: can be populated later if needed
        );

        return Task.FromResult(info);
    }

    private static string BuildConnectionString(int port)
    {
        List<string> addresses = ["127.0.0.1"];

        try
        {
            addresses.AddRange(System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Where(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !System.Net.IPAddress.IsLoopback(ip.Address))
                .Select(ip => ip.Address.ToString())
                .Distinct());
        }
        catch
        {
            // Fallback to loopback only
        }

        return $"{string.Join(",", addresses)}:{port}";
    }
}
