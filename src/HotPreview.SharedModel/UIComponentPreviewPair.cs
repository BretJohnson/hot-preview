namespace HotPreview.SharedModel;

public readonly struct UIComponentPreviewPair<TUIComponent, TPreview>(TUIComponent uiComponent, TPreview preview)
    where TPreview : PreviewBase
    where TUIComponent : UIComponentBase<TPreview>
{
    public TUIComponent UIComponent { get; } = uiComponent;

    public TPreview Preview { get; } = preview;
}
