namespace ExampleFramework;

public class UIComponentPreviewPair<TUIComponent, TPreview>(TUIComponent uiComponent, TPreview preview)
    where TPreview : ExampleBase
    where TUIComponent : UIComponentBase<TPreview>
{
    public TUIComponent UIComponent => uiComponent;

    public TPreview Example => preview;
}
