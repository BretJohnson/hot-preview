using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.PreviewFramework.App;

public abstract class UIPreviewAppService : IUIPreviewAppService
{
    public abstract Task NavigateToPreviewAsync(string uiComponentName, string previewName);
    //public abstract Task<ImageSnapshot> GetPreviewSnapshotAsync(string uiComponentName, string previewName);

    protected static UIComponentReflection GetUIComponent(string uiComponentName)
    {
        UIComponentsReflection uiComponents = UIPreviewsManagerReflection.Instance.UIComponents;
        return uiComponents.GetUIComponent(uiComponentName) ?? throw new UIComponentNotFoundException($"UIComponent {uiComponentName} not found");
    }

    public Task<string[]> GetUIComponentPreviewsAsync(string componentName)
    {
        UIComponentReflection component = GetUIComponent(componentName);
        string[] previewNames = component.Previews.Select(preview => preview.Name).ToArray();

        return Task.FromResult(previewNames);
    }

    protected static UIPreviewReflection GetPreview(string uiComponentName, string previewName)
    {
        UIComponentReflection uiComponent = GetUIComponent(uiComponentName);
        return uiComponent.GetPreview(previewName) ?? throw new PreviewNotFoundException($"Preview {previewName} not found for UIComponent {uiComponentName}");
    }
}
