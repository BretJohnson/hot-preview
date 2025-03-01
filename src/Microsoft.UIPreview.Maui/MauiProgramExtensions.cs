using Microsoft.Maui.Handlers;
using System.Diagnostics;

namespace Microsoft.UIPreview.Maui;

public static class MauiProgramExtensions
{
	public static MauiAppBuilder UsePreviewsOverlay(this MauiAppBuilder builder, Color? ribbonColor = null)
	{
		builder.ConfigureMauiHandlers(handlers =>
		{
			WindowHandler.Mapper.AppendToMapping("AddDebugOverlay", (handler, view) =>
            {
                Debug.WriteLine("Adding DebugOverlay");
                var overlay = new PreviewsWindowOverlay(handler.VirtualView, ribbonColor);
                handler.VirtualView.AddOverlay(overlay);
            });
		});

		return builder;
	}
}
