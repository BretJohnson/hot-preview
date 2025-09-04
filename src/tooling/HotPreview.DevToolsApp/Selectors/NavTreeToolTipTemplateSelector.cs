using HotPreview.DevToolsApp.ViewModels.NavTree;

namespace HotPreview.DevToolsApp.Selectors;

public class NavTreeToolTipTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UIComponentTemplate { get; set; }
    public DataTemplate? PreviewTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return item switch
        {
            UIComponentViewModel => UIComponentTemplate ?? base.SelectTemplateCore(item),
            PreviewViewModel => PreviewTemplate ?? base.SelectTemplateCore(item),
            _ => base.SelectTemplateCore(item)
        };
    }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}

