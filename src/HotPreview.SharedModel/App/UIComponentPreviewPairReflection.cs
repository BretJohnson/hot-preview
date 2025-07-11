namespace HotPreview.SharedModel.App;

public readonly struct UIComponentPreviewPairReflection(UIComponentReflection uiComponent, PreviewReflection preview)
{
    public UIComponentReflection UIComponent { get; } = uiComponent;
    public PreviewReflection Preview { get; } = preview;
}
