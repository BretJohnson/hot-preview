namespace Microsoft.UIPreview.Maui.ViewModels;

public class UIComponentCategoryViewModel : PreviewsItemViewModel
{
    public string Name { get; }

    public UIComponentCategoryViewModel(UIComponentCategory category)
    {
        Name = category.Name;
    }
}
