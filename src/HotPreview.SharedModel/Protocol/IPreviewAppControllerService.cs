using System.Threading.Tasks;

namespace HotPreview.SharedModel.Protocol;

internal interface IPreviewAppControllerService
{
    public Task RegisterAppAsync(string projectPath, string platformName);

    public Task NotifyPreviewsChangedAsync();
}
