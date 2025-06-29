using System;
using System.ComponentModel;
using ModelContextProtocol.Server;
using PreviewFramework.McpServer.Helpers;

namespace PreviewFramework.McpServer;

[McpServerToolType]
public class WindowsTools
{
    /// <summary>
    /// Runs a Windows cmd command on the current device.
    /// </summary>
    /// <param name="deviceSerial">The serial number of the device.</param>
    /// <param name="command">The shell command to execute.</param>
    /// <returns>A formatted string containing the command output.</returns>
    [McpServerTool(Name = "win_cmd_command")]
    [Description("Runs a Windows cmd command on user system.")]
    public string WinCmdCommand(string command)
    {
        try
        {
            string result = Process.ExecuteCommand($"{command}");

            // Check for security validation failure
            if (result.StartsWith("Error: Command rejected", StringComparison.OrdinalIgnoreCase))
            {
                return result;
            }

            // Format the output
            string output = $"# Command Output\n\nCommand was: {command}\n\n";
            output += $"```\n{result}\n```";

            return output;
        }
        catch (Exception ex)
        {
            return $"Error executing shell command: {ex.Message}";
        }
    }

    /// <summary>
    /// Builds a .NET MAUI projector specific platform on user system and returns build errors if any.
    /// </summary>
    /// <param name="projectPath">Full path to the .csproj file</param>
    /// <param name="configuration">Build configuration (Debug/Release)</param>
    /// <returns>Build result with errors if any</returns>
    [McpServerTool(Name = "win_build_maui")]
    [Description("Builds a .NET MAUI project for specific target framework on user system and returns errors if any.")]
    public string BuildMauiProject(string projectPath, string configuration, string targetFramework)
    {
        try
        {
            string buildCommand = $"dotnet build \"{projectPath}\" --configuration {configuration} -f {targetFramework}";
            string result = Process.ExecuteCommand($"{buildCommand}");

            // Check for build errors
            if (result.Contains("Build FAILED") || result.Contains("error"))
            {
                // Extract just the error messages
                string[] lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var errorLines = lines.Where(line => line.Contains("error") || line.Contains("warning")).ToList();

                string errors = string.Join("\n", errorLines);
                return $"# Build Failed\nCommand was: {buildCommand}\n```\n{errors}\n```";
            }

            return $"# Build Successful\n\n```\nProject built successfully: {projectPath}\n```";
        }
        catch (Exception ex)
        {
            return $"Error building project: {ex.Message}";
        }
    }

    /// <summary>
    /// Publishes a .NET MAUI project as a Windows MSIX package.
    /// </summary>
    /// <param name="projectPath"></param>
    /// <param name="configuration"></param>
    /// <param name="platform"></param>
    /// <returns></returns>
    [McpServerTool(Name = "win_publish_maui")]
    [Description("Publishes a .NET MAUI project for specific target framework on user system.")]
    public string PublishMauiProject(string projectPath, string configuration, string targetFramework)
    {
        try
        {
            string publishCommand = $"dotnet publish \"{projectPath}\" -f {targetFramework} -c {configuration}";
            string result = Process.ExecuteCommand($"{publishCommand}");

            // Check for build errors
            if (result.Contains("Publish FAILED") || result.Contains("error"))
            {
                // Extract just the error messages
                string[] lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var errorLines = lines.Where(line => line.Contains("error") || line.Contains("warning")).ToList();

                string errors = string.Join("\n", errorLines);
                return $"# Publish Failed\nCommand was: {publishCommand}\n\n```\n{errors}\n```";
            }

            // Try to find the publish directory
            if (result.Contains("Published"))
            {
                string[] lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var publishLine = lines.FirstOrDefault(line => line.Contains("Published") || line.Contains("publish"));

                if (publishLine != null)
                {
                    return $"# Publish Successful\n\n```\n{publishLine}\n```";
                }
            }

            return $"# Publish Successful\n\n```\nProject published successfully: {projectPath}\n```";
        }
        catch (Exception ex)
        {
            return $"Error publishing project: {ex.Message}";
        }
    }

}
