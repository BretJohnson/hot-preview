using System.ComponentModel;
using HotPreview.Tooling.McpServer.Helpers;
using HotPreview.Tooling.McpServer.Interfaces;
using HotPreview.Tooling.McpServer.Models;
using ModelContextProtocol.Server;

namespace HotPreview.Tooling.McpServer.Tools.Android;

/// <summary>
/// Executes ADB command with passed parameters.
/// </summary>
[McpServerToolType]
public class AndroidDeviceTool
{
    private readonly IProcessService _processService;

    public AndroidDeviceTool(IProcessService processService)
    {
        _processService = processService;
    }

    /*
     To do a prompt like  "click on a green button with a rabbit on it"..
     need to do a more descriptive prompt like
     "obtain connected device display size, then calculate from it the location of [description of the element] and click on it"
     So maybe we could add a special Tool for such task.
     */

    [McpServerTool(Name = "android_execute_adb")]
    [Description("Executes ADB command with passed parameters and returns the result.")]
    public string ExecAdb(string parameters)
    {
        try
        {
            // Execute the adb command to kill the emulator
            return _processService.ExecuteCommand($"adb {parameters}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error booting the device: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves a list of connected Android devices.
    /// </summary>
    /// <returns>
    /// A string containing the list of connected devices and their details, such as serial numbers.
    /// </returns>
    [McpServerTool(Name = "android_list_devices")]
    [Description("Lists all available Android devices.")]
    public string ListDevices()
    {
        try
        {
            if (!Adb.CheckAdbInstalled(_processService))
            {
                return "Error retrieving device list: ADB is not installed or not in PATH. Please install ADB and ensure it is in your PATH.";
            }

            var devices = new List<AdbDevice>();
            string result = _processService.ExecuteCommand("adb devices -l");

            string[] lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            // Skip the first line (header)
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];

                if (!string.IsNullOrWhiteSpace(line))
                {
                    // Parse each line to extract device details
                    string[] parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    var device = new AdbDevice
                    {
                        Product = GetPropertyFromParts(parts, "product:"),
                        Model = GetPropertyFromParts(parts, "model:"),
                        Device = GetPropertyFromParts(parts, "device:"),
                        SerialNumber = parts[0], // Assuming the serial number is the first part
                    };
                    devices.Add(device);
                }
            }

            if (devices is null || devices.Count == 0)
            {
                return "No devices found.";
            }

            // Format the result as a table
            string devicesStr = "# Devices\n\n";
            devicesStr += "| Serial          | Device           | Product          | Model            |\n";
            devicesStr += "|-----------------|------------------|------------------|------------------|\n";

            foreach (AdbDevice device in devices)
            {
                devicesStr += $"| `{device.SerialNumber}` | `{device.Device}` | `{device.Product}` | `{device.Model}` |\n";
            }

            return devicesStr;
        }
        catch (Exception ex)
        {
            return $"Error retrieving device list: {ex.Message}";
        }
    }

    /// <summary>
    /// Boots up an Android Virtual Device (AVD) emulator with the specified name.
    /// </summary>
    /// <param name="avdName">The name of the Android Virtual Device (AVD) to be booted.</param>
    [McpServerTool(Name = "android_boot_device")]
    [Description("Boots the specified Android device.")]
    public void BootDevice(string avdName)
    {
        if (string.IsNullOrEmpty(avdName))
        {
            throw new ArgumentNullException(nameof(avdName), "Error: Device name is missing or invalid.");
        }

        try
        {
            if (!Adb.CheckAdbInstalled(_processService))
            {
                throw new Exception("ADB is not installed or not in PATH. Please install ADB and ensure it is in your PATH.");
            }

            // Execute the adb command to kill the emulator
            _processService.ExecuteCommand($"adb -s {avdName} emu kill");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error booting the device: {ex.Message}");
        }
    }

    /// <summary>
    /// Shuts down an Android Virtual Device (AVD) emulator with the specified name.
    /// </summary>
    /// <param name="avdName">The name of the Android Virtual Device (AVD) to be shut down.</param>
    [McpServerTool(Name = "android_shutdown_device")]
    [Description("Shuts down the specified Android device.")]
    public void ShutdownDevice(string avdName)
    {
        if (string.IsNullOrEmpty(avdName))
        {
            throw new ArgumentNullException(nameof(avdName), "Error: Device name is missing or invalid.");
        }

        try
        {
            if (!Adb.CheckAdbInstalled(_processService))
            {
                throw new Exception("ADB is not installed or not in PATH. Please install ADB and ensure it is in your PATH.");
            }

            // Execute the adb command to kill the emulator
            _processService.ExecuteCommand($"adb -s {avdName} emu kill");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error shutting down the device: {ex.Message}");
        }
    }

    // Extracts the value of a specific property from an array of strings.
    private string GetPropertyFromParts(string[] parts, string propertyKey)
    {
        foreach (string part in parts)
        {
            if (part.StartsWith(propertyKey, StringComparison.OrdinalIgnoreCase))
            {
                return part.Substring(propertyKey.Length);
            }
        }

        return string.Empty;
    }
}
