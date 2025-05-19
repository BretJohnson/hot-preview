using Microsoft.Maui.Controls;
using ExampleFramework.Maui.ViewModels;

namespace ExampleFramework.Maui.Pages;

public class PreviewItemDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UIComponentTemplate { get; set; }
    public DataTemplate? PreviewTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
        (item is UIComponentViewModel) ? UIComponentTemplate! : PreviewTemplate!;
}
