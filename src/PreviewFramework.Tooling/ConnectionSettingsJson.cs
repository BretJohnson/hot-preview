using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace PreviewFramework.Tooling;

public static class ConnectionSettingsJson
{
    public static void WriteSettings(int appConnectionPort)
    {
        // Get all IPv4 addresses for the local machine (excluding loopback)
        var addresses = NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
            .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
            .Where(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip.Address))
            .Select(ip => ip.Address.ToString())
            .Distinct()
            .ToList();

        // If no addresses found, fallback to 127.0.0.1
        if (addresses.Count == 0)
            addresses.Add("127.0.0.1");

        string appConnectionString = string.Join(",", addresses.Select(ip => $"{ip}:{appConnectionPort}"));

        string jsonPath = GetFilePath(createDirectory: true);
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

    /// <summary>
    /// Gets the file path for the connection settings JSON file in the user's home directory.
    /// </summary>
    /// <param name="createDirectory">If true, creates the configuration directory if it doesn't exist.</param>
    /// <returns>The full path to the connectionSettings.json file in the .previewframework directory.</returns>
    private static string GetFilePath(bool createDirectory)
    {
        // Get the user's home directory
        string homeDir = Environment.GetFolderPath(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.SpecialFolder.UserProfile
                : Environment.SpecialFolder.Personal);

        string configDir = Path.Combine(homeDir, ".previewframework");

        if (createDirectory)
        {
            Directory.CreateDirectory(configDir);
        }

        return Path.Combine(configDir, "connectionSettings.json");
    }

    /// <summary>
    /// Reads the "app" connection string from the connection settings JSON file.
    /// </summary>
    /// <returns>The app connection string if found, otherwise null.</returns>
    public static string? GetAppConnectionString()
    {
        try
        {
            string jsonPath = GetFilePath(createDirectory: false);

            if (!File.Exists(jsonPath))
                return null;

            string jsonContent = File.ReadAllText(jsonPath);

            using JsonDocument document = JsonDocument.Parse(jsonContent);

            if (document.RootElement.TryGetProperty("app", out JsonElement appElement))
            {
                return appElement.GetString();
            }

            return null;
        }
        catch
        {
            // Return null if any error occurs (file not found, invalid JSON, etc.)
            return null;
        }
    }
}
