namespace Microsoft.UIPreview.Maui;

public class PreviewsWindowOverlayElement : IWindowOverlayElement
{
    readonly WindowOverlay overlay;
    private readonly Color badgeColor;
    private RectF badgeRect;

    public PreviewsWindowOverlayElement(WindowOverlay overlay, Color? badgeColor = null)
    {
        this.overlay = overlay;
        this.badgeColor = badgeColor ?? Colors.MediumPurple;
    }

    public bool Contains(Point point)
    {
        // For some reason (on Android at least) the coordinate system starts about 50 pixels higher than the canvas.
        // Compensate for that here
        Point offsetPoint = point.Offset(0, -50.0);
        return badgeRect.Contains(offsetPoint);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float badgeWidth = 20;

        badgeRect = new RectF(dirtyRect.Right - 10 - badgeWidth, dirtyRect.Top + 10, badgeWidth, badgeWidth);

        canvas.SaveState();

        // Draw the badge
        canvas.FillColor = badgeColor.WithAlpha((float)0.1);
        canvas.FillCircle(badgeRect.Center, badgeRect.Width / 2);

        /*
        // Draw the text
        canvas.FontColor = Colors.White.WithAlpha((float)0.4);
        canvas.FontSize = 12;
        canvas.Font = new Microsoft.Maui.Graphics.Font("ArialMT", 800, FontStyleType.Normal);
        canvas.DrawString("E", badgeRect, HorizontalAlignment.Center, VerticalAlignment.Center);
        */

        canvas.RestoreState();
    }
}
