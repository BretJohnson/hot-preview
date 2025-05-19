using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleFramework.App;

public abstract class ExampleAppService(ExampleApplication previewApplication) : IPreviewAppService
{
    public abstract Task NavigateToPreviewAsync(string uiComponentName, string previewName);

    protected UIComponentReflection GetUIComponent(string uiComponentName)
    {
        return previewApplication.GetUIComponentsManager().GetUIComponent(uiComponentName) ??
            throw new UIComponentNotFoundException($"UIComponent {uiComponentName} not found");
    }

    protected UIComponentReflection? GetUIComponentIfExists(string uiComponentName)
    {
        return previewApplication.GetUIComponentsManager().GetUIComponent(uiComponentName);
    }

    public Task<string[]> GetUIComponentPreviewsAsync(string componentName)
    {
        UIComponentReflection? component = GetUIComponentIfExists(componentName);
        if (component is null) {
            return Task.FromResult(Array.Empty<string>());
        }

        string[] previewNames = component.Previews.Select(preview => preview.Name).ToArray();
        return Task.FromResult(previewNames);
    }

    protected UIComponentPreviewPairReflection GetUIComponentExamplePair(string uiComponentName, string previewName)
    {
        UIComponentReflection uiComponent = GetUIComponent(uiComponentName);
        ExampleReflection preview = uiComponent.GetPreview(previewName) ?? throw new ExampleNotFoundException($"Example {previewName} not found for UIComponent {uiComponentName}");
        return new UIComponentPreviewPairReflection(uiComponent, preview);
    }
}
