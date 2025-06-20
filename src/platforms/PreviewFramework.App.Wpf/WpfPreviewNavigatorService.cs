using System;
using System.Threading.Tasks;
using PreviewFramework.Model.App;

namespace PreviewFramework.App.Wpf;

public class WpfPreviewNavigatorService
{
    public virtual void NavigateToPreview(UIComponentReflection uiComponent, PreviewReflection preview)
    {
        // TODO: Implement navigation
    }

    public virtual Task NavigateToPreviewAsync(UIComponentReflection uiComponent, PreviewReflection preview)
    {
        throw new NotImplementedException("WpfPreviewNavigatorService.NavigateToPreviewAsync is not implemented.");
    }
}
