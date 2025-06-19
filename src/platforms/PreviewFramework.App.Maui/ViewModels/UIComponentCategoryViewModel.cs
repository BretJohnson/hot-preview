using PreviewFramework.Model;

namespace PreviewFramework.App.Maui.ViewModels;

public class UIComponentCategoryViewModel : PreviewsItemViewModel
{
    public string Name { get; }

    public UIComponentCategoryViewModel(UIComponentCategory category)
    {
        Name = category.Name;
    }
}
