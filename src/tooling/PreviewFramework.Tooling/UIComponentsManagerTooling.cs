using PreviewFramework.SharedModel;

namespace PreviewFramework.Tooling;

public class UIComponentsManagerTooling(
    IReadOnlyDictionary<string, UIComponentTooling> uiComponents,
    IReadOnlyDictionary<string, UIComponentCategory> categories) : UIComponentsManagerBase<UIComponentTooling, PreviewTooling>(uiComponents, categories)
{
}
