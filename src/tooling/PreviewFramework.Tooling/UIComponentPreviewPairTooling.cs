using PreviewFramework.SharedModel;

namespace PreviewFramework.Tooling;

public class UIComponentPreviewPairTooling(UIComponentTooling uiComponent, PreviewTooling preview) : UIComponentPreviewPair<UIComponentTooling, PreviewTooling>(uiComponent, preview)
{
}
