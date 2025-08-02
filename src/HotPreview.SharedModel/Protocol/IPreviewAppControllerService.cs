using System.Threading.Tasks;

namespace HotPreview.SharedModel.Protocol;

public interface IPreviewAppControllerService
{
    public Task RegisterAppAsync(string projectPath, string platformName);

    public Task NotifyPreviewsChangedAsync();
}
