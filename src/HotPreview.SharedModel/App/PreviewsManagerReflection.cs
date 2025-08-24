using System.Collections.Generic;

namespace HotPreview.SharedModel.App;

public class PreviewsManagerReflection(
    IReadOnlyDictionary<string, UIComponentReflection> uiComponents,
    IReadOnlyDictionary<string, UIComponentCategory> categories,
    IReadOnlyDictionary<string, CommandReflection> commands) :
    PreviewsManagerBase<UIComponentReflection, PreviewReflection, CommandReflection>(uiComponents, categories, commands)
{
}
