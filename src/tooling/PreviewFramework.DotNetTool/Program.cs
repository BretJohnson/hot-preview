using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PreviewFramework.DotNetTool;

internal class Program
{
    private static async Task<int> Main(string[] args)
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

            // Look for the DevTools executable in the tools/devtools subdirectory
            // The CLI tool is installed in tools/net9.0/any, so we need to go up to tools and then to devtools
            string devToolsDirectory = Path.Combine(toolDirectory, "..", "..", "devtools");
            string? devToolsExecutable = GetDevToolsExecutablePath(devToolsDirectory);

            if (string.IsNullOrEmpty(devToolsExecutable) || !File.Exists(devToolsExecutable))
            {
                Console.Error.WriteLine($"Error: PreviewFramework.DevTools executable not found.");
                Console.Error.WriteLine($"Expected location: {devToolsDirectory}");
                return 1;
            }

            Console.WriteLine($"Launching PreviewFramework DevTools from: {devToolsExecutable}");

            // Launch the DevTools application
            ProcessStartInfo startInfo;

            if (devToolsExecutable.EndsWith(".dll"))
            {
                // Launch using dotnet
                startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{devToolsExecutable}\" {string.Join(" ", args)}",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };
            }
            else
            {
                // Launch executable directly
                startInfo = new ProcessStartInfo
                {
                    FileName = devToolsExecutable,
                    Arguments = string.Join(" ", args),
                    UseShellExecute = true,
                    CreateNoWindow = false
                };
            }

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.Error.WriteLine("Error: Failed to start PreviewFramework.DevTools.");
                return 1;
            }

            // Wait for the process to exit
            await process.WaitForExitAsync();
            return process.ExitCode;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error launching PreviewFramework.DevTools: {ex.Message}");
            return 1;
        }
    }

    private static string? GetDevToolsExecutablePath(string devToolsDirectory)
    {
        if (!Directory.Exists(devToolsDirectory))
        {
            return null;
        }

        // Look for the executable based on the platform
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string exePath = Path.Combine(devToolsDirectory, "PreviewFramework.DevToolsApp.exe");
            if (File.Exists(exePath))
                return exePath;
        }

        // Fallback to the .dll file (can be executed with dotnet)
        string dllPath = Path.Combine(devToolsDirectory, "PreviewFramework.DevToolsApp.dll");
        if (File.Exists(dllPath))
        {
            return dllPath;
        }

        return null;
    }
}
