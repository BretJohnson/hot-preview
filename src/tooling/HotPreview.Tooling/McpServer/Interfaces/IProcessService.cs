namespace HotPreview.Tooling.McpServer.Interfaces;

/// <summary>
/// Interface for process operations including command execution and process starting.
/// </summary>
public interface IProcessService
{
    /// <summary>
    /// Executes a shell command and returns the standard output as a string.
    /// </summary>
    /// <param name="command">The shell command to be executed.</param>
    /// <returns>The output from the executed command.</returns>
    /// <exception cref="Exception">
    /// Thrown when an error occurs during the command execution process.
    /// </exception>
    string ExecuteCommand(string command);

    /// <summary>
    /// Starts a new process to execute the specified shell command.
    /// </summary>
    /// <param name="command">The shell command to be executed.</param>
    /// <returns>
    /// The <see cref="System.Diagnostics.Process"/> instance representing the started process.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when an error occurs during the process startup.
    /// </exception>
    System.Diagnostics.Process StartProcess(string command);
}
