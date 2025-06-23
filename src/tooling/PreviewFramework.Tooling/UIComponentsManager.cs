using PreviewFramework.Model;

namespace PreviewFramework.Tooling;

public class UIComponentsManager : UIComponentsManagerBase<UIComponent, Preview>
{
    public UIComponentsManager(
        IReadOnlyDictionary<string, UIComponent> uiComponents,
        IReadOnlyDictionary<string, UIComponentCategory> categories)
        : base(uiComponents, categories)
    {
    }
}
