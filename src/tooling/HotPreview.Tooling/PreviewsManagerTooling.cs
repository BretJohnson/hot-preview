using HotPreview.SharedModel;

namespace HotPreview.Tooling;

public class PreviewsManagerTooling(
    IReadOnlyDictionary<string, UIComponentTooling> uiComponents,
    IReadOnlyDictionary<string, UIComponentCategory> categories,
    IReadOnlyDictionary<string, PreviewCommandTooling> commands) : PreviewsManagerBase<UIComponentTooling, PreviewTooling, PreviewCommandTooling>(uiComponents, categories, commands)
{
}
