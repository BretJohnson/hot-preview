using System.ComponentModel;
using HotPreview.Tooling.McpServer.Helpers;
using HotPreview.Tooling.McpServer.Interfaces;
using ModelContextProtocol.Server;

namespace HotPreview.Tooling.McpServer.Tools.iOS;

[McpServerToolType]
public class IosUiTool
{
    private readonly IProcessService _processService;

    public IosUiTool(IProcessService processService)
    {
        _processService = processService;
    }

    /// <summary>
    /// Simulates a tap gesture on the screen of an iOS device.
    /// This method requires the device's serial number and the screen coordinates (X, Y) as inputs.
    /// <paramref name="deviceId">The unique identifier (UDID) of the target iOS device.</paramref>
    /// <paramref name="x">The X coordinate of the screen where the tap should occur.</paramref>
    /// <paramref name="y">The Y coordinate of the screen where the tap should occur.</paramref>
    /// <returns>
    /// A string indicating the result of the tap operation.
    /// </returns>
    [McpServerTool(Name = "ios_ui_tap")]
    [Description("Simulates a tap gesture on the device screen at the specified coordinates (X, Y).")]
    public string Tap(string deviceId, int x, int y)
    {
        try
        {
            if (!Idb.CheckIdbInstalled(_processService))
            {
                throw new Exception("Idb is not installed or not in PATH. Please install Idb and ensure it is in your PATH.");
            }

            // Perform the tap operation
            _processService.ExecuteCommand($"idb ui tap --udid {deviceId} {x} {y}");

            return $"Successfully tapped at ({x}, {y})";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Performs a swipe gesture on the screen of a connected iOS device.
    /// </summary>
    /// <param name="deviceId">The unique identifier (UDID) of the target iOS device.</param>
    /// <param name="startX">The starting X coordinate of the swipe.</param>
    /// <param name="startY">The starting Y coordinate of the swipe.</param>
    /// <param name="endX">The ending X coordinate of the swipe.</param>
    /// <param name="endY">The ending Y coordinate of the swipe.</param>
    /// <param name="durationS">The duration of the swipe in seconds (default is 0.5).</param>
    /// <returns>
    /// A string indicating the result of the swipe operation.
    /// </returns>
    [McpServerTool(Name = "ios_ui_swipe")]
    [Description("Performs a swipe gesture on the screen of a connected iOS device.")]
    public string Swipe(string deviceId, int startX, int startY, int endX, int endY, double durationS = 0.5)
    {
        try
        {
            if (!Idb.CheckIdbInstalled(_processService))
            {
                throw new Exception("Idb is not installed or not in PATH. Please install Idb and ensure it is in your PATH.");
            }

            // Perform the swipe operation
            _processService.ExecuteCommand($"idb ui swipe --udid {deviceId} --duration {durationS} {startX},{startY} {endX},{endY}");

            return $"Successfully swiped from ({startX}, {startY}) to ({endX}, {endY})";
        }
        catch (Exception ex)
        {
            return $"Error executing swipe operation: {ex.Message}";
        }
    }

    /// <summary>
    /// Inputs text into a connected iOS device as if typed from a keyboard.
    /// </summary>
    /// <param name="deviceId">The unique identifier (UDID) of the target iOS device.</param>
    /// <param name="text">The text to be input into the device.</param>
    /// <returns>
    /// A string indicating the result of the text input operation.
    /// </returns>
    [McpServerTool(Name = "ios_ui_input_text")]
    [Description("Inputs text into a connected iOS device as if typed from a keyboard.")]
    public string InputText(string deviceId, string text)
    {
        try
        {
            if (!Idb.CheckIdbInstalled(_processService))
            {
                throw new Exception("Idb is not installed or not in PATH. Please install Idb and ensure it is in your PATH.");
            }

            // Perform the text input operation
            _processService.ExecuteCommand($"idb ui text ${text} --udid {deviceId}");

            return "Successfully input text on device.";
        }
        catch (Exception ex)
        {
            return $"Error executing text input operation: {ex.Message}";
        }
    }

    /// <summary>
    /// Simulates a key press on a connected iOS device identified by its UDID.
    /// </summary>
    /// <param name="deviceId">
    /// The unique identifier (UDID) of the iOS device on which the key press is to be performed.
    /// </param>
    /// <param name="keyCode">
    /// The code of the key to be pressed. This should match the key codes recognized by the device.
    /// </param>
    /// <returns>
    /// A message indicating whether the key press operation was successful or if an error occurred.
    /// </returns>
    /// <remarks>
    /// This method interacts with the iOS device using the `idb` tool to execute the key press operation.
    /// Ensure that the `idb` tool is properly installed and the specified device is accessible.
    /// </remarks>
    [McpServerTool(Name = "ios_ui_press_key")]
    [Description("Simulates pressing a specific key on an iOS device using its UDID and key code.")]
    public string PressKey(string deviceId, int keyCode)
    {
        try
        {
            if (!Idb.CheckIdbInstalled(_processService))
            {
                throw new Exception("Idb is not installed or not in PATH. Please install Idb and ensure it is in your PATH.");
            }

            // Execute the key press command
            _processService.ExecuteCommand($"idb ui key --udid {deviceId} {keyCode}");

            return "Key press operation completed successfully.";
        }
        catch (Exception e)
        {
            return $"An error occurred during the key press operation: {e.Message}";
        }
    }
}
