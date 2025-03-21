using Microsoft.Maui;

namespace Microsoft.UIPreview.Maui;

public class PreviewUIWindowOverlay : WindowOverlay
{
    private PreviewUIWindowOverlayElement _overlayElement;

    public PreviewUIWindowOverlay(IWindow window) : base(window)
    {
        _overlayElement = new PreviewUIWindowOverlayElement();
        AddWindowElement(_overlayElement);
        Tapped += PreviewsOverlay_Tapped;
    }

    private void PreviewsOverlay_Tapped(object? sender, WindowOverlayTappedEventArgs e)
    {
        if (_overlayElement.Contains(e.Point))
        {
            _ = MauiPreviewApplication.Instance.ShowPreviewUIAsync();

            /*
            // The tap is on the overlayElement
            this.RemoveWindowElement(overlayElement);
            overlayElement = new PreviewsWindowOverlayElement(this, badgeColor);
            this.AddWindowElement(overlayElement);
            */
        }
        else
        {
            // The tap is not on the overlayElement
        }
    }

    public override void HandleUIChange()
    {
        base.HandleUIChange();
    }
}
