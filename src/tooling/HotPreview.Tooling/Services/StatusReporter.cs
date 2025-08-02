namespace HotPreview.Tooling.Services;

/// <summary>
/// Service for reporting application status messages.
/// </summary>
public class StatusReporter
{
    /// <summary>
    /// Event raised when the status message changes.
    /// </summary>
    public event EventHandler<string>? StatusChanged;

    /// <summary>
    /// Updates the current status message.
    /// </summary>
    /// <param name="message">The status message to display.</param>
    public void UpdateStatus(string message)
    {
        StatusChanged?.Invoke(this, message);
    }

    /// <summary>
    /// Clears the status message (sets to default).
    /// </summary>
    public void ClearStatus()
    {
        UpdateStatus("Ready");
    }
}
