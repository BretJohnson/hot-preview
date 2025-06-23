using PreviewFramework.SharedModel;

namespace PreviewFramework.Tooling;

public class UIComponentPreviewPairTooling(UIComponentTooling uiComponent, Preview preview) : UIComponentPreviewPair<UIComponentTooling, Preview>(uiComponent, preview)
{
}
