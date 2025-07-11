using System;
using System.Linq;
using System.Threading.Tasks;
using HotPreview.SharedModel.Protocol;

namespace HotPreview.SharedModel.App;

public abstract class PreviewAppService(PreviewApplication previewApplication) : IPreviewAppService
{
    public PreviewApplication PreviewApplication { get; } = previewApplication;

    public abstract Task NavigateToPreviewAsync(string uiComponentName, string previewName);

    protected UIComponentReflection GetUIComponent(string uiComponentName)
    {
        return PreviewApplication.GetUIComponentsManager().GetUIComponent(uiComponentName) ??
            throw new UIComponentNotFoundException($"UIComponent {uiComponentName} not found");
    }

    protected UIComponentReflection? GetUIComponentIfExists(string uiComponentName)
    {
        return PreviewApplication.GetUIComponentsManager().GetUIComponent(uiComponentName);
    }

    public Task<string[]> GetUIComponentPreviewsAsync(string componentName)
    {
        UIComponentReflection? component = GetUIComponentIfExists(componentName);
        if (component is null)
        {
            return Task.FromResult(Array.Empty<string>());
        }

        string[] previewNames = component.Previews.Select(preview => preview.Name).ToArray();
        return Task.FromResult(previewNames);
    }

    public Task<UIComponentInfo[]> GetUIComponentsAsync()
    {
        UIComponentsManagerReflection uiComponentsManager = PreviewApplication.GetUIComponentsManager();

        UIComponentInfo[] uiComponentInfos = uiComponentsManager.UIComponents
            .Select(component => component.GetUIComponentInfo())
            .ToArray();

        return Task.FromResult(uiComponentInfos);
    }

    protected UIComponentPreviewPairReflection GetUIComponentPreviewPair(string uiComponentName, string previewName)
    {
        UIComponentReflection uiComponent = GetUIComponent(uiComponentName);
        PreviewReflection preview = uiComponent.GetPreview(previewName) ?? throw new PreviewNotFoundException($"Preview {previewName} not found for UIComponent {uiComponentName}");
        return new UIComponentPreviewPairReflection(uiComponent, preview);
    }
}
