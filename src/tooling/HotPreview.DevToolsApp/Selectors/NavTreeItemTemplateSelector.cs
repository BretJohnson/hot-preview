using HotPreview.DevToolsApp.ViewModels.NavTree;

namespace HotPreview.DevToolsApp.Selectors;

public class NavTreeItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? SectionItemTemplate { get; set; }
    public DataTemplate? DefaultItemTemplate { get; set; }
    public DataTemplate? CommandItemTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return item switch
        {
            SectionItemViewModel => SectionItemTemplate ?? DefaultItemTemplate!,
            CommandViewModel => CommandItemTemplate ?? DefaultItemTemplate!,
            _ => DefaultItemTemplate!
        };
    }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}
