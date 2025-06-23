using PreviewFramework.Model;

namespace PreviewFramework.Tooling;

public class UIComponent(UIComponentKind kind, string typeName, string? displayNameOverride, IReadOnlyList<Preview> previews) :
    UIComponentBase<Preview>(kind, displayNameOverride, previews)
{
    public override string Name => typeName;

    public override UIComponentBase<Preview> WithAddedPreview(Preview preview) =>
        new UIComponent(Kind, typeName, DisplayNameOverride, GetUpdatedPreviews(preview));
}
