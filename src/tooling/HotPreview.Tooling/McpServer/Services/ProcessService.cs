using System.Diagnostics;
using HotPreview.Tooling.McpServer.Interfaces;

namespace HotPreview.Tooling.McpServer.Services;

/// <summary>
/// Default implementation of IProcessService.
/// </summary>
public class ProcessService : IProcessService
{
    /// <summary>
    /// Executes a shell command and returns the standard output as a string.
    /// </summary>
    /// <param name="command">The shell command to be executed.</param>
    /// <returns>The output from the executed command.</returns>
    /// <exception cref="Exception">
    /// Thrown when an error occurs during the command execution process.
    /// </exception>
    public string ExecuteCommand(string command)
    {
        Process process = StartProcess(command);

        string output = process.StandardOutput.ReadToEnd();

        process.WaitForExit();

        return output;
    }

    /// <summary>
    /// Starts a new process to execute the specified shell command.
    /// </summary>
    /// <param name="command">The shell command to be executed.</param>
    /// <returns>
    /// The <see cref="Process"/> instance representing the started process.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when an error occurs during the process startup.
    /// </exception>
    public Process StartProcess(string command)
    {
        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/bash",
                Arguments = OperatingSystem.IsWindows() ? $"/C {command}" : $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        return process;
    }
}
