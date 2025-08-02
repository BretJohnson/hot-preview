using HotPreview.SharedModel;

namespace HotPreview.Tooling;

/// <summary>
/// Tooling-based command implementation for static methods discovered through Roslyn analysis.
/// </summary>
public class PreviewCommandTooling(string methodFullName, string? displayNameOverride) : PreviewCommandBase(displayNameOverride)
{
    /// <summary>
    /// The full qualified method name of the command.
    /// </summary>
    public override string Name => methodFullName;
}
