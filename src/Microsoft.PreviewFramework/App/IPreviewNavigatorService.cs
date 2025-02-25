using System.Threading.Tasks;

namespace Microsoft.PreviewFramework.App;

public interface IUIPreviewNavigatorService
{
    public Task NavigateToPreviewAsync(UIPreviewReflection preview);
}
