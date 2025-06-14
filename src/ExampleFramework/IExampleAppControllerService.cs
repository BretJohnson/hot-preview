using System.Threading.Tasks;

namespace ExampleFramework;

public interface IExampleAppControllerService
{
    public Task RegisterAppAsync(string? projectPath, string applicationName, string platformName);
}
