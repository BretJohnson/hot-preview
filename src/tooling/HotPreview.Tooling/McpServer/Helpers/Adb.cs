using HotPreview.Tooling.McpServer.Interfaces;

namespace HotPreview.Tooling.McpServer.Helpers;

public static class Adb
{
    /// <summary>
    /// Checks if ADB (Android Debug Bridge) is installed on the system.
    /// </summary>
    /// <param name="processService">The process service to use for executing commands.</param>
    /// <returns>True if ADB is installed; otherwise, false.</returns>
    public static bool CheckAdbInstalled(IProcessService processService)
    {
        try
        {
            System.Diagnostics.Process process = processService.StartProcess("adb version");
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
