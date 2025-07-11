using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace HotPreview.Tooling;

public static class ConnectionSettingsJson
{
    public static void WriteSettings(string fileName, int appConnectionPort)
    {
        List<string> addresses = ["127.0.0.1"];

        addresses.AddRange(NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
            .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
            .Where(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip.Address))
            .Select(ip => ip.Address.ToString())
            .Distinct());

        string appConnectionString = $"{string.Join(",", addresses)}:{appConnectionPort}";

        string homeDir = Environment.GetFolderPath(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.SpecialFolder.UserProfile
                : Environment.SpecialFolder.Personal);
        string configDir = Path.Combine(homeDir, ".hotpreview");
        Directory.CreateDirectory(configDir);
        string jsonPath = Path.Combine(configDir, fileName);

        File.WriteAllText(jsonPath, JsonSerializer.Serialize(new { app = appConnectionString },
            new JsonSerializerOptions { WriteIndented = true }));

        // Ensure the file is deleted when the app exits
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            try
            {
                if (File.Exists(jsonPath))
                {
                    File.Delete(jsonPath);
                }
            }
            catch
            {
                // ignored
            }
        };
    }
}
