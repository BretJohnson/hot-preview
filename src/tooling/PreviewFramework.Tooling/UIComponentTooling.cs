using PreviewFramework.SharedModel;

namespace PreviewFramework.Tooling;

public class UIComponentTooling(UIComponentKind kind, string typeName, string? displayNameOverride, IReadOnlyList<PreviewTooling> previews) :
    UIComponentBase<PreviewTooling>(kind, displayNameOverride, previews)
{
    public override string Name => typeName;

    public override UIComponentBase<PreviewTooling> WithAddedPreview(PreviewTooling preview) =>
        new UIComponentTooling(Kind, typeName, DisplayNameOverride, GetUpdatedPreviews(preview));
}
