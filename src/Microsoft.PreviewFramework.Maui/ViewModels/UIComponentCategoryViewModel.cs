using Microsoft.PreviewFramework.App;

namespace Microsoft.PreviewFramework.Maui.ViewModels;

public class UIComponentCategoryViewModel : List<object>
{
    public string Name { get; }

    public UIComponentCategoryViewModel(UIComponentCategory category, List<UIComponentReflection> uiComponents)
    {
        this.Name = category.Name;
        foreach (UIComponentReflection uiComponent in uiComponents)
        {
            this.Add(new UIComponentViewModel(uiComponent));

            foreach (UIPreviewReflection preview in uiComponent.Previews)
            {
                this.Add(new PreviewViewModel(preview));
            }
        }
    }
}
