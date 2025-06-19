using PreviewFramework.Model;

namespace PreviewFramework.Tooling;

public class UIComponentPreviewPair(UIComponent uiComponent, Preview preview) : UIComponentPreviewPair<UIComponent, Preview>(uiComponent, preview)
{
}
