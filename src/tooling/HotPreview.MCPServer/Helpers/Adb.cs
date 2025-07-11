using System.Diagnostics;

namespace HotPreview.McpServer.Helpers;

public static class Adb
{
    /// <summary>
    /// Checks if ADB (Android Debug Bridge) is installed on the system.
    /// </summary>
    /// <returns>True if ADB is installed; otherwise, false.</returns>
    public static bool CheckAdbInstalled()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "adb",
                    Arguments = "version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            return process.ExitCode == 0; // Return true if ADB is installed and the command succeeds.
        }
        catch
        {
            // Handle errors, e.g., if ADB is not found or the command fails.
            return false;
        }
    }
}
