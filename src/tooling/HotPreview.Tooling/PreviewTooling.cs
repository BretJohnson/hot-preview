using HotPreview.SharedModel;

namespace HotPreview.Tooling;

public abstract class PreviewTooling(string name, string? displayNameOverride) : PreviewBase(displayNameOverride)
{
    private readonly string _name = name;

    public override string Name => _name;
}
