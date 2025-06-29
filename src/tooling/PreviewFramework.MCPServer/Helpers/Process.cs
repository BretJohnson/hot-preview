using System.Diagnostics;

namespace MobileDevMcpServer.Helpers
{
    public static class Process
    {
        /// <summary>
        /// Executes a shell command and returns the standard output as a string.
        /// </summary>
        /// <param name="command">The shell command to be executed.</param>
        /// <returns>The output from the executed command.</returns>
        /// <exception cref="Exception">
        /// Thrown when an error occurs during the command execution process.
        /// </exception>
        public static string ExecuteCommand(string command)
        {
            var process = StartProcess(command);

            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            return output;
        }

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
        public static System.Diagnostics.Process StartProcess(string command)
        {
            var process = new System.Diagnostics.Process
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
}
