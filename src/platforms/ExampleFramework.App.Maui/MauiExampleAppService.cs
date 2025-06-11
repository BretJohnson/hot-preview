using System.Threading.Tasks;
using ExampleFramework.App;

namespace ExampleFramework.App.Maui;

public class MauiExampleAppService(MauiExampleApplication mauiExampleApplication) : ExampleAppService(mauiExampleApplication)
{
    public override async Task NavigateToExampleAsync(string uiComponentName, string exampleName)
    {
        UIComponentExamplePairReflection uiComponentExamplePair = GetUIComponentExamplePair(uiComponentName, exampleName);
        await MauiExampleApplication.Instance.ExampleNavigatorService.
            NavigateToExampleAsync(uiComponentExamplePair.UIComponent, uiComponentExamplePair.Example).ConfigureAwait(false);
    }
}
