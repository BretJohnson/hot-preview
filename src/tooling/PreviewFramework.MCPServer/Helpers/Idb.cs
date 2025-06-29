using System.Diagnostics;

namespace PreviewFramework.McpServer.Helpers;

public static class Idb
{
    public static bool CheckIdbInstalled()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "idb",
                    Arguments = "version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            return process.ExitCode == 0; // Return true if Idb is installed and the command succeeds.
        }
        catch
        {
            // Handle errors, e.g., if Idb is not found or the command fails.
            return false;
        }
    }
}
