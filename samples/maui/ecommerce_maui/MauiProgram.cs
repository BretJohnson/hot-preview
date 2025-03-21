using Camera.MAUI;
using CommunityToolkit.Maui;
using Plugin.Maui.DebugOverlay;

namespace EcommerceMAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCameraView()
            .UseDebugRibbon(Color.FromArgb("#FF3300"))
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Material-Icon.ttf", "MaterialIcon");
                fonts.AddFont("FontAwesome6-Brands.otf", "FA6Brands");
                fonts.AddFont("FontAwesome6-Regular.otf", "FA6Regular");
            });

        return builder.Build();
    }
}
