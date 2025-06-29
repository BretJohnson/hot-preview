using MobileDevMcpServer.Helpers;
using MobileDevMcpServer.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace MobileDevMcpServer
{
    [McpServerToolType]
    public class IosDeviceTool
    {
        /// <summary>
        /// Retrieves and lists all available simulator devices.
        /// </summary>
        /// <returns>
        /// A string formatted as a table, containing details of simulator devices such as name, ID, and runtime.
        /// If no devices are found or an error occurs, an appropriate message is returned.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when an error occurs during the device retrieval process.
        /// </exception>
        [McpServerTool(Name = "ios_list_devices")]
        [Description("Lists all available iOS simulator devices.")]
        public string ListDevices()
        {
            try
            {
                string resultJson = Process.ExecuteCommand("xcrun simctl list devices --json");

                // Deserialize JSON data into SimulatorDevices object
                var result = JsonSerializer.Deserialize<SimulatorDevices>(resultJson);

                if (result?.Devices is null || result.Devices.Count == 0)
                {
                    return "No simulator devices available.";
                }

                // Prepare table header
                var devicesTable = new StringBuilder("# Simulator Devices\n\n");
                devicesTable.AppendLine("| Name             | Udid                | Runtime       |");
                devicesTable.AppendLine("|------------------|---------------------|---------------|");

                // Process devices and format table rows
                foreach (var runtime in result.Devices)
                {
                    string runtimeName = runtime.Key.Replace("com.apple.CoreSimulator.SimRuntime.", string.Empty);

                    foreach (var device in runtime.Value)
                    {
                        device.Runtime = runtimeName;
                        devicesTable.AppendLine($"| {device.Name,-16} | {device.Udid,-20} | {runtimeName,-13} |");
                    }
                }

                return devicesTable.ToString();
            }
            catch (JsonException jsonEx)
            {
                return $"Error parsing simulator devices: {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                return $"Error retrieving simulator devices: {ex.Message}";
            }
        }

        /// <summary>
        /// Retrieves the name and ID of the first booted simulator device.
        /// </summary>
        /// <returns>
        /// The name and ID of the booted simulator device in a formatted string.
        /// If no booted simulator is found, an exception is thrown.
        /// </returns>
        [McpServerTool(Name = "ios_booted_device")]
        [Description("Retrieves the name and ID of the first booted simulator device.")]
        public static string GetBootedDevice()
        {
            try
            {
                string resultJson = Process.ExecuteCommand("xcrun simctl list devices --json");

                // Deserialize JSON data into SimulatorDevices object
                var result = JsonSerializer.Deserialize<SimulatorDevices>(resultJson);

                if (result?.Devices is null || result.Devices.Count == 0)
                {
                    return "No simulator devices available.";
                }

                // Process devices and format table rows
                foreach (var runtime in result.Devices)
                {
                    string runtimeName = runtime.Key.Replace("com.apple.CoreSimulator.SimRuntime.", string.Empty);

                    foreach (var device in runtime.Value)
                    {
                         if (device.State == "Booted")
                        {
                            // Format the result as a table
                            var deviceStr = "# Booted Device\n\n";
                            deviceStr += "| Name            | Udid              |\n";
                            deviceStr += "|-----------------|-----------------|\n";
                            deviceStr += $"| {device.Name} | {device.Udid} |\n";

                            return deviceStr;
                        }
                    }
                }

                return "No booted devices found.";
            }
            catch (Exception ex)
            {
                return $"Error retrieving booted device: {ex.Message}";
            }
        }

        /// <summary>
        /// Boots a simulator device with the specified device ID.
        /// </summary>
        /// <param name="deviceId">The unique identifier of the simulator device to be booted.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the provided <paramref name="deviceId"/> is null or empty.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when an error occurs during the boot operation.
        /// </exception>
        [McpServerTool(Name = "ios_boot_device")]
        [Description("Boots the specified iOS simulator device.")]
        public void BootDevice(string deviceId)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId))
                {
                    throw new ArgumentNullException(nameof(deviceId), "Error: Invalid or missing device ID.");
                }

                // Execute the command to boot the simulator device
                Process.ExecuteCommand($"xcrun simctl boot {deviceId}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error booting the simulator device: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Shuts down a simulator device with the specified device ID.
        /// </summary>
        /// <param name="deviceId">The unique identifier of the simulator device to be shut down.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the provided <paramref name="deviceId"/> is null or empty.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when an error occurs during the shutdown operation.
        /// </exception>
        [McpServerTool(Name = "ios_shutdown_device")]
        [Description("Shuts down the specified iOS simulator device.")]
        public void ShutdownDevice(string deviceId)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId))
                {
                    throw new ArgumentNullException(nameof(deviceId), "Error: Invalid or missing device ID.");
                }

                // Execute the command to shut down the simulator device
                Process.ExecuteCommand($"xcrun simctl shutdown {deviceId}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error shutting down the simulator device: {ex.Message}", ex);
            }
        }
    }
}
