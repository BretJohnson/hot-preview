namespace HotPreview;

/// <summary>
/// Designates this method as a command that can be executed from the DevTools UI or as an MCP tool command.
/// </summary>
/// <remarks>
/// <para>Commands typically update global state, changing the way that subsequent previews appear. For instance,
/// commands could update the UI language for the app or switch the theme between light and dark. Commands normally
/// don't update UI themselves (but they can - nothing prevents this).</para>
/// <para>Commands for now should have no parameters and return void, though likely we'll add parameter and return
/// support in the future once we've figured out the desired behavior.</para>
/// </remarks>
/// <param name="displayName">Optional display name override for the command, determining how it appears in navigation
/// UI. If not specified, the name of the method is used.</param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class PreviewCommandAttribute(string? displayName = null) : Attribute
{
    /// <summary>
    /// Optional display name override for the command, determining how it appears in navigation UI.
    /// If not specified, the name of the method is used.
    /// </summary>
    public string? DisplayName { get; } = displayName;
}
