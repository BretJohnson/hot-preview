using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Microsoft.UIPreview.Maui;

public class PreviewUIWindowOverlayElement : IWindowOverlayElement
{
    private RectF badgeRect;

    public bool Contains(Point point)
    {
        // For some reason (on Android at least) the coordinate system starts about 50 pixels higher than the canvas.
        // Compensate for that here
        Point offsetPoint = point.Offset(0, -50.0);
        return badgeRect.Contains(offsetPoint);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        Color? badgeColor = MauiPreviewApplication.Instance.PreviewUIBadgeColor;
        if (badgeColor == null)
        {
            return;
        }

        float badgeWidth = 10;
        badgeRect = new RectF(dirtyRect.Right - 5 - badgeWidth, dirtyRect.Top + 5, badgeWidth, badgeWidth);

        canvas.SaveState();

        // Draw the badge
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
