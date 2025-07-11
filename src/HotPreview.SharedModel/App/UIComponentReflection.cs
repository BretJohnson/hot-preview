using System;
using System.Collections.Generic;
using System.Linq;
using PreviewFramework.SharedModel.Protocol;

namespace PreviewFramework.SharedModel.App;

public class UIComponentReflection(Type type, UIComponentKind kind, string? displayNameOverride, IReadOnlyList<PreviewReflection> previews) :
    UIComponentBase<PreviewReflection>(kind, displayNameOverride, previews)
{
    public Type Type { get; } = type;

    public override string Name => Type.FullName;

    public override UIComponentBase<PreviewReflection> WithAddedPreview(PreviewReflection preview) =>
        new UIComponentReflection(Type, Kind, DisplayNameOverride, GetUpdatedPreviews(preview));

    /// <summary>
    /// Gets the UI component information including name, display name, and preview information.
    /// </summary>
    /// <returns>A UIComponentInfo record with the component details, for use in the JSON RPC protocol</returns>
    public UIComponentInfo GetUIComponentInfo()
    {
        return new UIComponentInfo(
            Name: Name,
            UIComponentKindInfo: UIComponentKindInfo.FromUIComponentKind(Kind),
            DisplayName: DisplayNameOverride,
            Previews: Previews.Select(preview => preview.GetPreviewInfo()).ToArray());
    }
}
