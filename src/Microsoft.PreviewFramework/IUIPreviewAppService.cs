using System.Threading.Tasks;

namespace Microsoft.PreviewFramework;

public interface IUIPreviewAppService
{
    public Task NavigateToPreviewAsync(string componentName, string previewName);
    //public Task<ImageSnapshot> GetPreviewSnapshotAsync(string componentName, string previewName);
    public Task<string[]> GetUIComponentPreviewsAsync(string componentName);
}
