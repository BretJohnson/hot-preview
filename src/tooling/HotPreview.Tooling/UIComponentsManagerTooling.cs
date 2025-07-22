using HotPreview.SharedModel;

namespace HotPreview.Tooling;

public class UIComponentsManagerTooling(
    IReadOnlyDictionary<string, UIComponentTooling> uiComponents,
    IReadOnlyDictionary<string, UIComponentCategory> categories,
    IReadOnlyDictionary<string, PreviewCommandTooling> commands) : UIComponentsManagerBase<UIComponentTooling, PreviewTooling, PreviewCommandTooling>(uiComponents, categories, commands)
{
}
