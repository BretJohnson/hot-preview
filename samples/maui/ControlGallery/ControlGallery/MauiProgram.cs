using ControlGallery.Data;

namespace ControlGallery;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        MauiAppBuilder builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

#if PREVIEWS
        builder.EnablePreviewMode<App>();
#endif
        builder.Services.AddSingleton<WeatherForecastService>();

        return builder.Build();
	}
}
