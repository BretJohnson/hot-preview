using System.Threading.Tasks;

namespace HotPreview.SharedModel.Protocol;

public interface IPreviewAppService
{
    public Task<UIComponentInfo[]> GetUIComponentsAsync();

    public Task<string[]> GetUIComponentPreviewsAsync(string componentName);

    public Task NavigateToPreviewAsync(string componentName, string previewName);

    public Task<byte[]> GetPreviewSnapshotAsync(string uiComponentName, string previewName);

    public Task<string[]> GetCommandsAsync();

    public Task InvokeCommandAsync(string commandName);
}
