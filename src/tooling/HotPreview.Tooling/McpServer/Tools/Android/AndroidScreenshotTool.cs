using System;
using System.ComponentModel;
using System.IO;
using HotPreview.Tooling.McpServer.Helpers;
using HotPreview.Tooling.McpServer.Interfaces;
using ModelContextProtocol.Server;

namespace HotPreview.Tooling.McpServer;

[McpServerToolType]
public class AndroidScreenshotTool
{
    private readonly IProcessService _processService;

    public AndroidScreenshotTool(IProcessService processService)
    {
        _processService = processService;
    }
    /// <summary>
    /// Captures a screenshot from the specified Android device and retrieves the image data as a byte array.
    /// </summary>
    /// <param name="deviceSerial">The serial number of the target Android device. This is required if multiple devices are connected.</param>
    /// <returns>
    /// A byte array containing the screenshot image data, or null if the operation fails or the device is not found.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the provided <paramref name="deviceSerial"/> is null or empty.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown when an error occurs during the screenshot capturing process or ADB command execution.
    /// </exception>
    [McpServerTool(Name = "android_screenshot")]
    [Description("Captures a screenshot from the specified Android device.")]
    public byte[]? TakeScreenshot(string deviceSerial)
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

            // Define the temporary file path on the device
            string deviceTempFilePath = "/sdcard/screenshot.png";

            // Take the screenshot on the device
            _processService.ExecuteCommand($"adb -s {deviceSerial} shell screencap -p {deviceTempFilePath}");

            // Define a temporary file path to save the pulled screenshot locally
            string localTempFilePath = Path.GetTempFileName();

            // Pull the screenshot from the device to the local machine
            _processService.ExecuteCommand($"adb pull {deviceTempFilePath} \"{localTempFilePath}\"");

            // Delete the screenshot file from the device
            _processService.ExecuteCommand($"adb shell rm {deviceTempFilePath}");

            // Read the screenshot image data into a byte array
            byte[] imageData = File.ReadAllBytes(localTempFilePath);

            // Delete the temporary file after reading
            File.Delete(localTempFilePath);

            return imageData;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
