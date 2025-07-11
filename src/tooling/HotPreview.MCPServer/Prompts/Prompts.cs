using System.ComponentModel;
using ModelContextProtocol.Server;

namespace PreviewFramework.McpServer;

[McpServerPromptType]
public class Prompts
{
    /// <summary>
    /// Generates a debugging prompt for analyzing application logs of an Android app.
    /// This method creates a detailed request to assist with log analysis, including capturing relevant logs,
    /// diagnosing issues, and suggesting solutions.
    /// </summary>
    /// <param name="packageName">
    /// The package name of the Android application whose logs need debugging.
    /// Example: "com.example.myapp" represents the unique identifier of the app.
    /// </param>
    /// <returns>
    /// A string containing a structured prompt to guide log analysis for the specified app.
    /// The prompt includes steps for capturing logcat output, analyzing stack traces, identifying issues,
    /// and suggesting potential fixes.
    /// </returns>
    [McpServerPrompt]
    [Description("Generates a debugging prompt for analyzing Android application logs.")]
    public string DebugAppLogs(string packageName)
    {
        return $@"I need help debugging a crash in my Android app with package name '{packageName}'.

            Can you help me:
            1. Capture the logcat output for this app.
            2. Analyze the stack trace
            3. Identify the error messages.
            4. Find the the root cause of the crash between the error messages.
            4. Suggest potential fixes.

            Please include suggestion changes that might solve the issue.";
    }
}
