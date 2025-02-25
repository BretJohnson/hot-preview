using Microsoft.PreviewFramework.App;

namespace Microsoft.PreviewFramework.Maui.ViewModels;

public class UIComponentViewModel
{
    public string DisplayName => this.UIComponent.DisplayName;

    public UIComponentReflection UIComponent { get; }

    public UIComponentViewModel(UIComponentReflection uiComponent)
    {
        this.UIComponent = uiComponent;
    }
}
