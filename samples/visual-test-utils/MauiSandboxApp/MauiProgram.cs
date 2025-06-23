using Microsoft.Extensions.Logging;
using VisualTestUtils.AppConnector.App;

namespace MauiSandboxApp
{
    public static class MauiProgram
    {
        private static AppConnectorApp? _appConnectorApp;

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

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        public static void StartAppConnector()
        {
            if (_appConnectorApp == null)
            {
                _appConnectorApp = new AppConnectorApp(new AppService());
                _ = _appConnectorApp.StartClientAsync();
            }
        }
    }
}
