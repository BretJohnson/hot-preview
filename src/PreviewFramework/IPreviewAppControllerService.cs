using System.Threading.Tasks;

namespace PreviewFramework;

public interface IPreviewAppControllerService
{
    public Task RegisterAppAsync(string projectPath, string platformName);
}
