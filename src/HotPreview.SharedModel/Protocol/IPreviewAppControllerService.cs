using System.Threading.Tasks;

namespace PreviewFramework.SharedModel.Protocol;

public interface IPreviewAppControllerService
{
    public Task RegisterAppAsync(string projectPath, string platformName);

    public Task NotifyUIComponentsChangedAsync();
}
