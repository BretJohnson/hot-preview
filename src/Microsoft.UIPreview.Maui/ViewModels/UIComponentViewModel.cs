using Microsoft.UIPreview.App;

namespace Microsoft.UIPreview.Maui.ViewModels;

public class UIComponentViewModel
{
    public string DisplayName => this.UIComponent.DisplayName;

    public UIComponentReflection UIComponent { get; }

    public UIComponentViewModel(UIComponentReflection uiComponent)
    {
        this.UIComponent = uiComponent;
    }
}
