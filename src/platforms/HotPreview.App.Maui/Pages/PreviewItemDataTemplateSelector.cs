using HotPreview.App.Maui.ViewModels;
using Microsoft.Maui.Controls;

namespace HotPreview.App.Maui.Pages;

public class PreviewItemDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UIComponentTemplate { get; set; }
    public DataTemplate? PreviewTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
        (item is UIComponentViewModel) ? UIComponentTemplate! : PreviewTemplate!;
}
