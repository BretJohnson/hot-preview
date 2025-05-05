using static Microsoft.UIPreview.Maui.PreviewExtensions;

namespace DefaultTemplateWithContent.Pages.Controls;

public partial class CategoryChart
{
    public CategoryChart()
    {
        InitializeComponent();
    }

#if PREVIEWS
    [Preview]
    public static CategoryChart Preview() => CreateViewWithBindingToService<CategoryChart, MainPageModel>();
#endif
}
