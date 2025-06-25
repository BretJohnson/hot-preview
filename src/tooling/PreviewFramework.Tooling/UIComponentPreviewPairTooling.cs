namespace PreviewFramework.Tooling;

public readonly struct UIComponentPreviewPairTooling(UIComponentTooling uiComponent, PreviewTooling preview)
{
    public UIComponentTooling UIComponent { get; } = uiComponent;
    public PreviewTooling Preview { get; } = preview;
}
