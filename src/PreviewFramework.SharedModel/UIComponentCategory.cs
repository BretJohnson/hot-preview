using System.Collections.Generic;

namespace PreviewFramework.SharedModel;

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
}
