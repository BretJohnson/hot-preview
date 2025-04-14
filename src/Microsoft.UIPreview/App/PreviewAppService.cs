using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.UIPreview.App;

public abstract class PreviewAppService : IPreviewAppService
{
    public abstract Task NavigateToPreviewAsync(string uiComponentName, string previewName);

    protected static UIComponentReflection GetUIComponent(string uiComponentName)
    {
        UIComponentsManagerReflection uiComponentsManager = UIComponentsManagerReflection.Instance;
        return uiComponentsManager.GetUIComponent(uiComponentName) ?? throw new UIComponentNotFoundException($"UIComponent {uiComponentName} not found");
    }

    protected static UIComponentReflection? GetUIComponentIfExists(string uiComponentName)
    {
        UIComponentsManagerReflection uiComponentsManager = UIComponentsManagerReflection.Instance;
        return uiComponentsManager.GetUIComponent(uiComponentName);
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

    protected static PreviewReflection GetPreview(string uiComponentName, string previewName)
    {
        UIComponentReflection uiComponent = GetUIComponent(uiComponentName);
        return uiComponent.GetPreview(previewName) ?? throw new PreviewNotFoundException($"Preview {previewName} not found for UIComponent {uiComponentName}");
    }
}
