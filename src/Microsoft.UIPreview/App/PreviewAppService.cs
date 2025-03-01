using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.UIPreview.App;

public abstract class PreviewAppService : IPreviewAppService
{
    public abstract Task NavigateToPreviewAsync(string uiComponentName, string previewName);
    //public abstract Task<ImageSnapshot> GetPreviewSnapshotAsync(string uiComponentName, string previewName);

    protected static UIComponentReflection GetUIComponent(string uiComponentName)
    {
        UIComponentsReflection uiComponents = PreviewsManagerReflection.Instance.UIComponents;
        return uiComponents.GetUIComponent(uiComponentName) ?? throw new UIComponentNotFoundException($"UIComponent {uiComponentName} not found");
    }

    public Task<string[]> GetUIComponentPreviewsAsync(string componentName)
    {
        UIComponentReflection component = GetUIComponent(componentName);
        string[] previewNames = component.Previews.Select(preview => preview.Name).ToArray();

        return Task.FromResult(previewNames);
    }

    protected static PreviewReflection GetPreview(string uiComponentName, string previewName)
    {
        UIComponentReflection uiComponent = GetUIComponent(uiComponentName);
        return uiComponent.GetPreview(previewName) ?? throw new PreviewNotFoundException($"Preview {previewName} not found for UIComponent {uiComponentName}");
    }
}
