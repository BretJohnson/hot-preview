using System;
using System.Collections.Generic;
using System.Linq;
using HotPreview.SharedModel.Protocol;

namespace HotPreview.SharedModel.App;

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
            name: Name,
            uiComponentKind: UIComponentKindInfo.FromUIComponentKind(Kind),
            displayName: DisplayNameOverride,
            previews: Previews.Select(preview => preview.GetPreviewInfo()).ToArray());
    }
}
