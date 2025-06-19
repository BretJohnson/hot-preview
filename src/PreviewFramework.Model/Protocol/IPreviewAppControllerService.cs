using System.Threading.Tasks;

namespace PreviewFramework.Model.Protocol;

public interface IPreviewAppControllerService
{
    public Task RegisterAppAsync(string projectPath, string platformName);
}
