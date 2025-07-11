using System;
using System.ComponentModel;
using System.IO;
using ModelContextProtocol.Server;
using PreviewFramework.McpServer.Helpers;

namespace PreviewFramework.McpServer;

[McpServerToolType]
public class AndroidDiagnosticsTool
{
    /// <summary>
    /// Captures a comprehensive bug report from a connected Android device.
    /// A bug report includes diagnostic information such as system logs, running processes, and device state.
    /// </summary>
    /// <param name="deviceSerial">The serial number of the target Android device.</param>
    /// <param name="outputPath">The file path where the bug report should be saved. If not provided, a temporary path will be used.</param>
    /// <param name="timeoutSeconds">The maximum duration allowed for the bug report process, in seconds (default is 500 seconds).</param>
    /// <returns>
    /// A string indicating the success or failure of the bug report operation, including the output file path or relevant error message.
    /// </returns>
    [McpServerTool(Name = "android_diagnostics_bug_report")]
    [Description("Captures a comprehensive bug report from a connected Android device.")]
    public string CaptureBugReport(string deviceSerial, string outputPath = "", int timeoutSeconds = 500)
    {
        try
        {
            if (!Adb.CheckAdbInstalled())
            {
                throw new Exception("ADB is not installed or not in PATH. Please install ADB and ensure it is in your PATH.");
            }

            if (string.IsNullOrEmpty(deviceSerial))
            {
                return "Error: Invalid or missing device serial number.";
            }

            // If output path is not provided, generate a temporary file
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                string tempDir = Path.GetTempPath();
                outputPath = Path.Combine(tempDir, $"bugreport_{deviceSerial}.zip");
            }

            // Ensure the output directory exists
            string? outputDir = Path.GetDirectoryName(outputPath);
            if (outputDir == null)
            {
                return "Error: Invalid output path provided for bug report.";
            }

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Run bug report command using ADB
            var process = Process.StartProcess($"adb -s {deviceSerial} bugreport {outputPath}");

            // Wait for process to complete or timeout
            if (!process.WaitForExit(timeoutSeconds * 1000))
            {
                process.Kill();
                return $"Error: Bug report capture exceeded the time limit of {timeoutSeconds} seconds.";
            }

            // Check process exit code
            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                return $"Error: Failed to capture bug report. ADB message: {error}";
            }

            // Verify if output file exists
            if (!File.Exists(outputPath))
            {
                return "Error: Bug report could not be saved because the output file was not generated.";
            }

            long fileSize = new FileInfo(outputPath).Length;
            double fileSizeMb = fileSize / (1024.0 * 1024.0);

            return $"Bug report completed successfully. Saved to: {outputPath} ({fileSizeMb:F2} MB)";
        }
        catch (Exception ex)
        {
            return $"Error: Failed to capture bug report. Details: {ex.Message}";
        }
    }
}
