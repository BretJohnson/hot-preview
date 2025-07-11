using System.Collections.Generic;

namespace HotPreview.SharedModel.App;

public class UIComponentsManagerReflection(
    IReadOnlyDictionary<string, UIComponentReflection> uiComponents,
    IReadOnlyDictionary<string, UIComponentCategory> categories) : UIComponentsManagerBase<UIComponentReflection, PreviewReflection>(uiComponents, categories)
{
}
