using System;
using System.ComponentModel;
using HotPreview.Tooling.McpServer.Helpers;
using HotPreview.Tooling.McpServer.Interfaces;
using ModelContextProtocol.Server;

namespace HotPreview.Tooling.McpServer;

[McpServerToolType]
public class IosAppManagementTool
{
    private readonly IProcessService _processService;

    public IosAppManagementTool(IProcessService processService)
    {
        _processService = processService;
    }

    /// <summary>
    /// Installs an application on a specified simulator device.
    /// </summary>
    /// <param name="deviceId">The unique identifier of the simulator device.</param>
    /// <param name="appPath">The file path to the application to be installed.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the provided <paramref name="deviceId"/> or <paramref name="appPath"/> is null or empty.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown when an error occurs during the installation process.
    /// </exception>
    [McpServerTool(Name = "ios_install_app")]
    [Description("Installs an application on the specified iOS simulator device.")]
    public void InstallApp(string deviceId, string appPath)
    {
        try
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentNullException(nameof(deviceId), "Error: Invalid or missing device ID.");
            }

            if (string.IsNullOrEmpty(appPath))
            {
                throw new ArgumentNullException(nameof(appPath), "Error: Invalid or missing application path.");
            }

            // Execute the command to install the application
            _processService.ExecuteCommand($"xcrun simctl install {deviceId} \"{appPath}\"");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error installing application: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Launches an application on a specified simulator device.
    /// </summary>
    /// <param name="deviceId">The unique identifier of the simulator device.</param>
    /// <param name="bundleId">The bundle identifier of the application to be launched.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the provided <paramref name="deviceId"/> or <paramref name="bundleId"/> is null or empty.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown when an error occurs during the launch process.
    /// </exception>
    [McpServerTool(Name = "ios_launch_app")]
    [Description("Launches an application on the specified iOS simulator device.")]
    public void LaunchApp(string deviceId, string bundleId)
    {
        try
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentNullException(nameof(deviceId), "Error: Invalid or missing device ID.");
            }

            if (string.IsNullOrEmpty(bundleId))
            {
                throw new ArgumentNullException(nameof(bundleId), "Error: Invalid or missing Bundle ID.");
            }

            // Execute the command to launch the application
            _processService.ExecuteCommand($"xcrun simctl launch {deviceId} {bundleId}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error launching application: {ex.Message}", ex);
        }
    }
}
