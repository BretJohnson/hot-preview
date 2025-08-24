using System.Collections.Generic;
using System.Linq;
using HotPreview.SharedModel.Protocol;

namespace HotPreview.SharedModel;

public class UIComponentCategory(string name, IReadOnlyList<string> uiComponentNames)
{
    public string Name { get; } = name;

    public IReadOnlyList<string> UIComponentNames { get; } = uiComponentNames;

    public UIComponentCategory WithAddedUIComponentNames(IReadOnlyList<string> additionalUIComponentNames)
    {
        var combinedNames = new List<string>(UIComponentNames.Count + additionalUIComponentNames.Count);
        combinedNames.AddRange(UIComponentNames);
        combinedNames.AddRange(additionalUIComponentNames);
        return new UIComponentCategory(Name, combinedNames);
    }

    /// <summary>
    /// Converts this UIComponentCategory to a UIComponentCategoryInfo for the JSON RPC protocol.
    /// </summary>
    /// <returns>A UIComponentCategoryInfo record with the category details, for use in the JSON RPC protocol</returns>
    public UIComponentCategoryInfo GetUIComponentCategoryInfo()
    {
        return new UIComponentCategoryInfo(Name, UIComponentNames.ToArray());
    }
}
