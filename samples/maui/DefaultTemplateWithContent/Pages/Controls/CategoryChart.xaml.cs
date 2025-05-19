namespace DefaultTemplateWithContent.Pages.Controls;

public partial class CategoryChart
{
    public CategoryChart()
    {
        InitializeComponent();
    }

#if EXAMPLES
    [Example]
    public static CategoryChart Example() => CreateViewWithBindingToService<CategoryChart, MainPageModel>();
#endif
}
