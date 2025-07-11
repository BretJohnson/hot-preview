using System.Diagnostics;
using System.Reflection;

namespace HotPreview.DevTools;

public static class Program
{
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

            // Look for the DevToolsApp executable in the tools/app subdirectory
            // The CLI tool is installed in tools/net9.0/any, so we need to go up to tools and then to app
            string? parentDirectory = Directory.GetParent(toolDirectory)?.FullName;
            string? toolsDirectory = parentDirectory is not null ? Directory.GetParent(parentDirectory)?.FullName : null;
            if (string.IsNullOrEmpty(toolsDirectory))
            {
                Console.Error.WriteLine("Error: Could not determine 'tools' directory structure.");
                return 1;
            }

            string devToolsAppDirectory = Path.Combine(toolsDirectory, "app");
            string devToolsAppExecutable = Path.Combine(devToolsAppDirectory, "HotPreview.DevToolsApp.exe");

            if (!File.Exists(devToolsAppExecutable))
            {
                Console.Error.WriteLine($"Error: HotPreview.DevToolsApp.exe not found.");
                Console.Error.WriteLine($"Expected location: {devToolsAppExecutable}");
                return 1;
            }

            Console.WriteLine($"Launching HotPreview DevToolsApp from: {devToolsAppExecutable}");

            // Launch the DevToolsApp application in the background
            var startInfo = new ProcessStartInfo
            {
                FileName = devToolsAppExecutable,
                Arguments = string.Join(" ", args),
                UseShellExecute = true,
                CreateNoWindow = false
            };

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                Console.Error.WriteLine("Error: Failed to start HotPreview.DevToolsApp.exe.");
                return 1;
            }

            // Don't wait for the process to exit - run in background
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error launching HotPreview.DevToolsApp.exe: {ex.Message}");
            return 1;
        }
    }
}
