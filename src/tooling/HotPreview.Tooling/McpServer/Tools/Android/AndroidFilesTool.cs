using System.ComponentModel;
using HotPreview.Tooling.McpServer.Helpers;
using HotPreview.Tooling.McpServer.Interfaces;
using ModelContextProtocol.Server;

namespace HotPreview.Tooling.McpServer.Tools.Android;

[McpServerToolType]
public class AndroidFilesTool
{
    private readonly IProcessService _processService;

    public AndroidFilesTool(IProcessService processService)
    {
        _processService = processService;
    }

    /// <summary>
    /// Pushes a local file to an Android device using ADB (Android Debug Bridge).
    /// </summary>
    /// <param name="deviceSerial">The serial number of the target device.</param>
    /// <param name="localPath">The local file path to be uploaded.</param>
    /// <param name="devicePath">The destination file path on the device.</param>
    /// <returns>
    /// A string message indicating the result of the operation:
    /// - A success message if the file upload completes successfully.
    /// - An error message if an exception occurs during the operation.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if:
    /// - ADB is not installed or not in the system PATH.
    /// - The device serial is null or empty.
    /// - The specified local file does not exist.
    /// </exception>
    [McpServerTool(Name = "android_files_push")]
    [Description("Pushes a local file to an Android device.")]
    public string PushFile(string deviceSerial, string localPath, string devicePath)
    {
        try
        {
            if (!Adb.CheckAdbInstalled(_processService))
            {
                throw new Exception("ADB is not installed or not in PATH. Please install ADB and ensure it is in your PATH.");
            }

            if (string.IsNullOrEmpty(deviceSerial))
            {
                throw new Exception($"Error: Device not found.");
            }

            // Check if the local file exists
            if (!File.Exists(localPath))
            {
                throw new Exception($"Error: Local file {localPath} does not exist.");
            }

            _processService.ExecuteCommand($"adb push -s {deviceSerial} {localPath} {devicePath}");

            return $"File Uploaded Successfully. The file `{localPath}` has been uploaded to `{devicePath}` on device {deviceSerial}.";
        }
        catch (Exception ex)
        {
            return $"Error pushing file: {ex.Message}";
        }
    }

    /// <summary>
    /// Pulls a file from an Android device to the local machine using ADB (Android Debug Bridge).
    /// </summary>
    /// <param name="deviceSerial">The serial number of the target device.</param>
    /// <param name="localPath">The destination path on the local machine where the file will be saved.</param>
    /// <param name="devicePath">The path of the file on the device that will be downloaded.</param>
    /// <returns>A message indicating the result of the file pull operation.</returns>
    [McpServerTool(Name = "android_files_push")]
    [Description("Pulls a file from an Android device to the local machine.")]
    public string PullFile(string deviceSerial, string localPath, string devicePath)
    {
        try
        {
            if (!Adb.CheckAdbInstalled(_processService))
            {
                throw new Exception("ADB is not installed or not in PATH. Please install ADB and ensure it is in your PATH.");
            }

            if (string.IsNullOrEmpty(deviceSerial))
            {
                throw new Exception($"Error: Device not found.");
            }

            // Check if the local file exists
            if (!File.Exists(localPath))
            {
                throw new Exception($"Error: Local file {localPath} does not exist.");
            }

            _processService.ExecuteCommand($"adb pull -s {deviceSerial} {devicePath} {localPath}");

            return $"File Downloaded Successfully. The file `{devicePath}` has been downloaded to `{localPath}` from device {deviceSerial}.";
        }
        catch (Exception ex)
        {
            return $"Error pushing file: {ex.Message}";
        }
    }

    /// <summary>
    /// Deletes a file or directory from the connected Android device using ADB (Android Debug Bridge).
    /// </summary>
    /// <param name="deviceSerial">The serial number of the target device.</param>
    /// <param name="path">The path of the file or directory to delete on the device.</param>
    /// <returns>A string message indicating the success or failure of the deletion operation.</returns>
    [McpServerTool(Name = "android_files_delete_file")]
    [Description("This tool allows you to delete a specified file from a connected Android device.")]
    public string DeleteFile(string deviceSerial, string path)
    {
        try
        {
            if (!Adb.CheckAdbInstalled(_processService))
            {
                throw new Exception("ADB is not installed or not in PATH. Please install ADB and ensure it is in your PATH.");
            }

            if (string.IsNullOrEmpty(deviceSerial))
            {
                throw new Exception($"Error: Device not found.");
            }

            string result = _processService.ExecuteCommand($"adb -s {deviceSerial} shell rm {path}");

            return $"File Deleted Successfully: {result}";

        }
        catch (Exception ex)
        {
            return $"Error deleting file: {ex.Message}";
        }
    }
}
