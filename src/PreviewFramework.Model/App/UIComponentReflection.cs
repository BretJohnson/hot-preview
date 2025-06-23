using System;
using System.Linq;
using PreviewFramework.Model.Protocol;

namespace PreviewFramework.Model.App;

public class UIComponentReflection : UIComponentBase<PreviewReflection>
{
    internal UIComponentReflection(Type type, UIComponentKind kind, string? displayNameOverride) : base(kind, displayNameOverride)
    {
        Type = type;
    }

    public Type Type { get; }

    public override string Name => Type.FullName;

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
