namespace PreviewFramework;

public class UIComponentPreviewPair<TUIComponent, TPreview>(TUIComponent uiComponent, TPreview preview)
    where TPreview : PreviewBase
    where TUIComponent : UIComponentBase<TPreview>
{
    public TUIComponent UIComponent => uiComponent;

    public TPreview Preview => preview;
}
