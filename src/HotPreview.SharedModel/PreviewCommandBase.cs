namespace HotPreview.SharedModel;

/// <summary>
/// Base class for command reflection, representing commands that can be executed from the DevTools UI.
/// </summary>
public abstract class PreviewCommandBase(string? displayNameOverride)
{
    private readonly string? _displayNameOverride = displayNameOverride;

    /// <summary>
    /// DisplayName is intended to be what's shown in UI to identify the command. It can contain spaces and
    /// isn't necessarily unique. It defaults to the command method name (with no namespace) but can
    /// be overridden by the developer.
    /// </summary>
    public string DisplayName => _displayNameOverride ?? NameUtilities.GetSimpleName(Name);

    public string? DisplayNameOverride => _displayNameOverride;

    /// <summary>
    /// Name is intended to be used by code to uniquely identify the command. It's the command's
    /// full qualified method name.
    /// </summary>
    public abstract string Name { get; }
}
