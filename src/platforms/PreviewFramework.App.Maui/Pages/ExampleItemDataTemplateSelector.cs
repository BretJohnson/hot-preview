using ExampleFramework.App.Maui.ViewModels;
using Microsoft.Maui.Controls;

namespace ExampleFramework.App.Maui.Pages;

public class ExampleItemDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UIComponentTemplate { get; set; }
    public DataTemplate? ExampleTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
        (item is UIComponentViewModel) ? UIComponentTemplate! : ExampleTemplate!;
}
