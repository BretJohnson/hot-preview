using System;
using System.Collections.Generic;
using System.ComponentModel;
using HotPreview.Tooling.McpServer.Helpers;
using HotPreview.Tooling.McpServer.Interfaces;
using ModelContextProtocol.Server;

namespace HotPreview.Tooling.McpServer;

[McpServerToolType]
public class AndroidAppManagementTool
{
    private readonly IProcessService _processService;

    public AndroidAppManagementTool(IProcessService processService)
    {
        _processService = processService;
    }
    /// <summary>
    /// Installs an application (APK file) on the specified Android device.
    /// </summary>
    /// <param name="deviceSerial">The serial number of the target Android device.</param>
    /// <param name="appPath">The file path to the APK file to be installed on the device.</param>
    [McpServerTool(Name = "android_install_app")]
    [Description("Installs an application on the specified Android emulator device.")]
    public void InstallApp(string deviceSerial, string appPath)
    {
        try
        {
            if (!Adb.CheckAdbInstalled(_processService))
            {
                throw new Exception("ADB is not installed or not in PATH. Please install ADB and ensure it is in your PATH.");
            }

            if (string.IsNullOrEmpty(deviceSerial))
            {
                throw new ArgumentNullException(nameof(deviceSerial), "Error: Invalid or missing device serial number.");
            }

            if (string.IsNullOrEmpty(appPath))
            {
                throw new ArgumentNullException(nameof(appPath), "Error: Invalid or missing application path.");
            }

            // Execute the adb install command
            _processService.ExecuteCommand($"adb -s {deviceSerial} install \"{appPath}\"");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error installing application: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Launches an application on the specified Android device.
    /// </summary>
    /// <param name="deviceSerial">The serial number of the target Android device.</param>
    /// <param name="packageName">The package name of the application to be launched.</param>
    [McpServerTool(Name = "android_launch_app")]
    [Description("Launches an application on the specified Android emulator device.")]
    public void LaunchApp(string deviceSerial, string packageName)
    {
        try
        {
            if (!Adb.CheckAdbInstalled(_processService))
            {
                throw new Exception("ADB is not installed or not in PATH. Please install ADB and ensure it is in your PATH.");
            }

            if (string.IsNullOrEmpty(deviceSerial))
            {
                throw new ArgumentNullException(nameof(deviceSerial), "Error: Invalid or missing device serial number.");
            }

            if (string.IsNullOrEmpty(packageName))
            {
                throw new ArgumentNullException(nameof(packageName), "Error: Invalid or missing package name.");
            }

            // Execute the ADB command to launch the app
            _processService.ExecuteCommand($"adb -s {deviceSerial} shell monkey -p {packageName} 1");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error launching application: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves a list of installed application packages from the specified Android device.
    /// </summary>
    /// <param name="deviceSerial">The serial number of the target Android device.</param>
    /// <returns>
    /// A string containing the list of installed application packages.
    /// </returns>
    [McpServerTool(Name = "android_list_packages")]
    [Description("Retrieves the list of installed package names from the specified Android device.")]
    public string ListPackages(string deviceSerial)
    {
        try
        {
            if (!Adb.CheckAdbInstalled(_processService))
            {
                throw new Exception("ADB is not installed or not in PATH. Please install ADB and ensure it is in your PATH.");
            }

            if (string.IsNullOrEmpty(deviceSerial))
            {
                return $"Error: Invalid or missing device serial number.";
            }

            var packages = new List<string>();

            string result = _processService.ExecuteCommand($"adb -s {deviceSerial} shell pm list packages");

            string[] lines = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                if (line.StartsWith("package:"))
                {
                    // Remove the "package:" prefix and add the package name to the list
                    string packageName = line.Replace("package:", "").Trim();
                    packages.Add(packageName);
                }
            }

            if (packages is null || packages.Count == 0)
            {
                return "No packages found on the device.";
            }

            // Format the result as a table
            string packagesStr = "# Installed Packages\n\n";
            packagesStr += "| Package Name |\n";
            packagesStr += "|--------------|\n";

            foreach (string app in packages)
            {
                packagesStr += $"| `{app}` |\n";
            }

            return packagesStr;
        }
        catch (Exception ex)
        {
            return $"Error retrieving packages: {ex.Message}";
        }
    }
}
