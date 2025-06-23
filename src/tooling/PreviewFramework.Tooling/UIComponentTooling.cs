using PreviewFramework.SharedModel;

namespace PreviewFramework.Tooling;

public class UIComponentTooling(UIComponentKind kind, string typeName, string? displayNameOverride, IReadOnlyList<Preview> previews) :
    UIComponentBase<Preview>(kind, displayNameOverride, previews)
{
    public override string Name => typeName;

    public override UIComponentBase<Preview> WithAddedPreview(Preview preview) =>
        new UIComponentTooling(Kind, typeName, DisplayNameOverride, GetUpdatedPreviews(preview));
}
