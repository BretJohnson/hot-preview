using System.Collections.Generic;

namespace PreviewFramework.Model.App;

public class UIComponentsManagerReflection(
    IReadOnlyDictionary<string, UIComponentReflection> uiComponents,
    IReadOnlyDictionary<string, UIComponentCategory> categories) : UIComponentsManagerBase<UIComponentReflection, PreviewReflection>(uiComponents, categories)
{
}
