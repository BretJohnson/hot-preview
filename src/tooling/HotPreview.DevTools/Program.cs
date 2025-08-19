using System.Diagnostics;
using System.Reflection;

namespace HotPreview.DevTools;

public static class Program
{
    /// <summary>
    /// Entry point for the "hot-preview" CLI tool, installed via "dotnet tool install".
    /// This tool acts as a launcher that finds and starts HotPreview.DevToolsApp.
    /// It may offer other CLI options in the future.
    /// </summary>
    public static int Main(string[] args)
    {
        try
        {
            // Get the directory where this tool is installed
            string? toolDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(toolDirectory))
            {
                Console.Error.WriteLine("Error: Could not determine tool installation directory.");
                return 1;
            }

            // Look for the DevToolsApp assembly in the tools/app subdirectory
            // The CLI tool is installed in tools/net9.0/any, so we need to go up to tools and then to app
            string? parentDirectory = Directory.GetParent(toolDirectory)?.FullName;
            string? toolsDirectory = parentDirectory is not null ? Directory.GetParent(parentDirectory)?.FullName : null;
            if (string.IsNullOrEmpty(toolsDirectory))
            {
                Console.Error.WriteLine("Error: Could not determine 'tools' directory structure.");
                return 1;
            }

            string devToolsAppDirectory = Path.Combine(toolsDirectory, "app");
            string devToolsAppAssembly = Path.Combine(devToolsAppDirectory, "HotPreview.DevToolsApp.dll");

            if (!File.Exists(devToolsAppAssembly))
            {
                Console.Error.WriteLine($"Error: HotPreview.DevToolsApp.dll not found.");
                Console.Error.WriteLine($"Expected location: {devToolsAppAssembly}");
                return 1;
            }

            Console.WriteLine($"Launching Hot Preview DevTools: dotnet {devToolsAppAssembly}");

            // Launch the DevTools application, with "dotnet". This works on all platforms (Windows, Linux, macOS),
            // as compared to directly executing the .exe, which is Windows only.
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = devToolsAppAssembly,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                Console.Error.WriteLine($"Error: Failed to start: dotnet {devToolsAppAssembly}");
                return 1;
            }

            // Exit the launcher, while the process keeps running
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error launching Hot Preview DevTools: {ex.Message}");
            return 1;
        }
    }
}
