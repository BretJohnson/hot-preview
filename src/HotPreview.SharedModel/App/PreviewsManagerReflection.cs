using System.Collections.Generic;

namespace HotPreview.SharedModel.App;

public class PreviewsManagerReflection(
    IReadOnlyDictionary<string, UIComponentReflection> uiComponents,
    IReadOnlyDictionary<string, UIComponentCategory> categories,
    IReadOnlyDictionary<string, PreviewCommandReflection> commands) :
    PreviewsManagerBase<UIComponentReflection, PreviewReflection, PreviewCommandReflection>(uiComponents, categories, commands)
{
}
