using System.Collections.Generic;

namespace HotPreview.SharedModel.App;

public class UIComponentsManagerReflection(
    IReadOnlyDictionary<string, UIComponentReflection> uiComponents,
    IReadOnlyDictionary<string, UIComponentCategory> categories,
    IReadOnlyDictionary<string, PreviewCommandReflection> commands) :
    UIComponentsManagerBase<UIComponentReflection, PreviewReflection, PreviewCommandReflection>(uiComponents, categories, commands)
{
}
