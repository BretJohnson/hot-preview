using System.Diagnostics;
using HotPreview.Tooling.McpServer.Interfaces;

namespace HotPreview.Tooling.McpServer.Helpers;

public static class Idb
{
    /// <summary>
    /// Checks if idb (iOS Debug Bridge) is installed on the system.
    /// </summary>
    /// <param name="processService">The process service to use for executing commands.</param>
    /// <returns>True if IDB is installed; otherwise, false.</returns>
    public static bool CheckIdbInstalled(IProcessService processService)
    {
        try
        {
            Process process = processService.StartProcess("idb version");
            process.WaitForExit();

            return process.ExitCode == 0; // Return true if idb is installed and the command succeeds.
        }
        catch
        {
            // Handle errors, e.g., if idb is not found or the command fails.
            return false;
        }
    }
}
