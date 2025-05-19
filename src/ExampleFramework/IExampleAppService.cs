using System.Threading.Tasks;

namespace ExampleFramework;

public interface IExampleAppService
{
    public Task NavigateToExampleAsync(string componentName, string exampleName);

    public Task<string[]> GetUIComponentExamplesAsync(string componentName);
}
