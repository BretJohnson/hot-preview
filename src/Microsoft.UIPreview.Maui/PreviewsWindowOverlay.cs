using System.Diagnostics;

namespace Microsoft.UIPreview.Maui;

public class PreviewsWindowOverlay : WindowOverlay
{
    private PreviewsWindowOverlayElement _overlayElement;

    private Color? _badgeColor;

    public PreviewsWindowOverlay(IWindow window, Color? badgeColor = null) : base(window)
    {
        _badgeColor = badgeColor;
        _overlayElement = new PreviewsWindowOverlayElement(this, badgeColor: badgeColor);
        AddWindowElement(_overlayElement);
        Tapped += PreviewsOverlay_Tapped;
    }

    private void PreviewsOverlay_Tapped(object? sender, WindowOverlayTappedEventArgs e)
    {
        if (_overlayElement.Contains(e.Point))
        {
            _ = MauiPreviewsApplication.Instance.ShowPreviewsAsync();

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
