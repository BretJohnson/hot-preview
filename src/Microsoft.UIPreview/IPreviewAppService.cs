using System.Threading.Tasks;

namespace Microsoft.UIPreview;

public interface IPreviewAppService
{
    public Task NavigateToPreviewAsync(string componentName, string previewName);

    public Task<string[]> GetUIComponentPreviewsAsync(string componentName);
}
