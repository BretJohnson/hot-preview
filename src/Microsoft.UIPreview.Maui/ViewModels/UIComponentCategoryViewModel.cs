using Microsoft.UIPreview.App;

namespace Microsoft.UIPreview.Maui.ViewModels;

public class UIComponentCategoryViewModel : List<object>
{
    public string Name { get; }

    public UIComponentCategoryViewModel(UIComponentCategory category, List<UIComponentReflection> uiComponents)
    {
        this.Name = category.Name;
        foreach (UIComponentReflection uiComponent in uiComponents)
        {
            this.Add(new UIComponentViewModel(uiComponent));

            foreach (PreviewReflection preview in uiComponent.Previews)
            {
                this.Add(new PreviewViewModel(preview));
            }
        }
    }
}
