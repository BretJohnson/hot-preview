using System.ComponentModel;
using System.Diagnostics;
using HotPreview.Tooling.McpServer.Interfaces;
using ModelContextProtocol.Server;

namespace HotPreview.Tooling.McpServer.Tools.iOS;

[McpServerToolType]
public class IosVideoRecordingTool
{
    private readonly IProcessService _processService;

    public IosVideoRecordingTool(IProcessService processService)
    {
        _processService = processService;
    }

    /// <summary>
    /// Records a video of the iOS Simulator using simctl directly.
    /// </summary>
    /// <param name="deviceId">The unique identifier (UDID) of the target iOS device.</param>
    /// <param name="path">Optional output path for the video file. Defaults to "~/Downloads/simulator_recording_$DATE.mp4".</param>
    /// <param name="codec">Specifies the codec type: "h264" or "hevc". Default is "hevc".</param>
    /// <param name="display">Display to capture: "internal" or "external". Default depends on device type.</param>
    /// <param name="mask">For non-rectangular displays, handle the mask by policy: "ignored", "alpha", or "black".</param>
    /// <param name="force">Force the output file to be written to, even if the file already exists.</param>
    /// <returns>A message indicating the result of the recording operation.</returns>
    [McpServerTool(Name = "ios_video_start")]
    [Description("Records a video of the iOS Simulator.")]
    public async Task<string> RecordVideoAsync(
        string deviceId,
        string? path = null,
        string codec = "hevc",
        string? display = null,
        string? mask = null,
        bool force = false)
    {
        try
        {
            string defaultFileName = $"simulator_recording_{DateTime.Now:yyyyMMddHHmmss}.mp4";
            string outputFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                path ?? defaultFileName
            );

            string command = $"xcrun simctl io {deviceId} recordVideo";

            if (!string.IsNullOrEmpty(codec))
                command += $" --codec={codec}";

            if (!string.IsNullOrEmpty(display))
                command += $" --display={display}";

            if (!string.IsNullOrEmpty(mask))
                command += $" --mask={mask}";

            if (force)
                command += " --force";

            command += $" {outputFile}";

            // Start the recording process
            Process recordingProcess = _processService.StartProcess(command);

            if (recordingProcess == null)
                throw new Exception("Failed to start the recording process.");

            string errorOutput = string.Empty;

            await Task.Run(() =>
            {
                recordingProcess.ErrorDataReceived += (sender, data) =>
                {
                    if (data.Data?.Contains("Recording started") ?? false)
                    {
                        return;
                    }
                    errorOutput += data.Data;
                };

                recordingProcess.BeginErrorReadLine();
            });

            await Task.Delay(3000);

            // Recording process stops unexpectedly
            if (recordingProcess.HasExited)
            {
                throw new Exception("Recording process terminated unexpectedly.");
            }

            return $"Recording started. The video will be saved to: {outputFile}\nTo stop recording, use the stop_recording command.";
        }
        catch (Exception ex)
        {
            return $"Error starting recording: {ex.Message}";
        }
    }

    /// <summary>
    /// Stops the simulator video recording by sending a SIGINT signal to the recording process.
    /// </summary>
    /// <returns>A message indicating the result of the stop recording operation.</returns>
    [McpServerTool(Name = "ios_video_stop")]
    [Description("Stops the simulator video recording.")]
    public async Task<string> StopRecordingAsync()
    {
        try
        {
            Process stopRecordingProcess = _processService.StartProcess("pkill -SIGINT -f \"simctl.*recordVideo\"");

            if (stopRecordingProcess == null)
                throw new Exception("Failed to start the pkill process.");

            await stopRecordingProcess.WaitForExitAsync();

            if (stopRecordingProcess.ExitCode != 0)
            {
                string errorOutput = await stopRecordingProcess.StandardError.ReadToEndAsync();
                throw new Exception($"pkill command failed with error: {errorOutput}");
            }

            // Wait for the video to finalize
            await Task.Delay(1000);

            return "Recording stopped successfully.";
        }
        catch (Exception ex)
        {
            return $"Error stopping recording: {ex.Message}";
        }
    }
}
