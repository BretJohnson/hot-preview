using System.Threading.Tasks;

namespace Microsoft.UIPreview.App;

public interface IPreviewNavigatorService
{
    public Task NavigateToPreviewAsync(PreviewReflection preview);
}
