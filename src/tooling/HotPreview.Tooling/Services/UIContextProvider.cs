namespace HotPreview.Tooling.Services;

/// <summary>
/// Provides access to the UI thread synchronization context.
/// </summary>
/// <param name="uiContext">The UI thread synchronization context.</param>
public class UIContextProvider(SynchronizationContext uiContext)
{
    /// <summary>
    /// Gets the UI thread synchronization context.
    /// </summary>
    public SynchronizationContext UIContext { get; } = uiContext;
}
