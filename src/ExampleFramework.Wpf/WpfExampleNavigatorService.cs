using ExampleFramework.App;

namespace ExampleFramework.Wpf;

public class WpfExampleNavigatorService
{
    public virtual void NavigateToExample(UIComponentReflection uiComponent, ExampleReflection example)
    {
        _ = NavigateToExampleAsync(uiComponent, example);
    }

    public virtual async Task NavigateToExampleAsync(UIComponentReflection uiComponent, ExampleReflection example)
    {
        throw new NotImplementedException("WpfExampleNavigatorService.NavigateToExampleAsync is not implemented.");
    }
}
