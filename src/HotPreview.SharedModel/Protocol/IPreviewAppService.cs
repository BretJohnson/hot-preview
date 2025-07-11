using System.Threading.Tasks;

namespace HotPreview.SharedModel.Protocol;

public interface IPreviewAppService
{
    public Task NavigateToPreviewAsync(string componentName, string previewName);

    public Task<string[]> GetUIComponentPreviewsAsync(string componentName);

    public Task<UIComponentInfo[]> GetUIComponentsAsync();
}
