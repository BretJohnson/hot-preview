using System;
using System.Threading.Tasks;
using HotPreview.SharedModel;
using HotPreview.SharedModel.App;

namespace HotPreview.App.Wpf;

public class WpfPreviewNavigatorService : IPreviewNavigator
{
    public virtual void NavigateToPreview(UIComponentReflection uiComponent, PreviewReflection preview)
    {
        // TODO: Implement navigation
    }

    public virtual Task NavigateToPreviewAsync(UIComponentReflection uiComponent, PreviewReflection preview)
    {
        throw new NotImplementedException("WpfPreviewNavigatorService.NavigateToPreviewAsync is not implemented.");
    }

    public virtual Task<byte[]> GetPreviewSnapshotAsync(UIComponentReflection uiComponent, PreviewReflection preview)
    {
        object? previewUI = preview.Create();

        if (uiComponent.Kind == UIComponentKind.Control)
        {
            return CaptureControlAsPngAsync(previewUI);
        }
        else
        {
            if (previewUI is System.Windows.Window window)
            {
                return CaptureWindowAsPngAsync(window);
            }
            else if (previewUI is System.Windows.Controls.Page page)
            {
                return CapturePageAsPngAsync(page);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported preview UI type: {previewUI?.GetType()}");
            }
        }
    }

    private Task<byte[]> CaptureControlAsPngAsync(object? control)
    {
        throw new NotImplementedException("WPF Control PNG capture is not yet implemented. Platform-specific implementation needed.");
    }

    private Task<byte[]> CaptureWindowAsPngAsync(System.Windows.Window window)
    {
        throw new NotImplementedException("WPF Window PNG capture is not yet implemented. Platform-specific implementation needed.");
    }

    private Task<byte[]> CapturePageAsPngAsync(System.Windows.Controls.Page page)
    {
        throw new NotImplementedException("WPF Page PNG capture is not yet implemented. Platform-specific implementation needed.");
    }
}
