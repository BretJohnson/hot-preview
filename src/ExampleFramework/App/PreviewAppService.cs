using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.UIPreview.App;

public abstract class PreviewAppService(PreviewApplication previewApplication) : IPreviewAppService
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

    protected UIComponentPreviewPairReflection GetUIComponentPreviewPair(string uiComponentName, string previewName)
    {
        UIComponentReflection uiComponent = GetUIComponent(uiComponentName);
        PreviewReflection preview = uiComponent.GetPreview(previewName) ?? throw new PreviewNotFoundException($"Preview {previewName} not found for UIComponent {uiComponentName}");
        return new UIComponentPreviewPairReflection(uiComponent, preview);
    }
}
