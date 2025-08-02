using System.Threading.Tasks;

namespace HotPreview.SharedModel.App;

public interface IPreviewNavigator
{
    Task NavigateToPreviewAsync(UIComponentReflection uiComponent, PreviewReflection preview);

    Task<byte[]> GetPreviewSnapshotAsync(UIComponentReflection uiComponent, PreviewReflection preview);
}
