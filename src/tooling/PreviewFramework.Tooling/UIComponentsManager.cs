using PreviewFramework.Model;

namespace PreviewFramework.Tooling;

public class UIComponentsManager(
    IReadOnlyDictionary<string, UIComponent> uiComponents,
    IReadOnlyDictionary<string, UIComponentCategory> categories) : UIComponentsManagerBase<UIComponent, Preview>(uiComponents, categories)
{
}
