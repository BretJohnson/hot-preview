using System.Diagnostics;
using Microsoft.UIPreview.Maui;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using WeatherTwentyOne.Models;
using WeatherTwentyOne.Services;
using WeatherTwentyOne.ViewModels;
using Application = Microsoft.Maui.Controls.Application;
using WindowsConfiguration = Microsoft.Maui.Controls.PlatformConfiguration.Windows;

namespace WeatherTwentyOne.Pages;

public partial class HomePage : ContentPage
{
    static bool isSetup = false;

    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;

        if (!isSetup)
        {
            isSetup = true;

            SetupAppActions();
            SetupTrayIcon();
        }
    }

    private void SetupAppActions()
    {
        try
        {
#if WINDOWS
            //AppActions.IconDirectory = Application.Current.On<WindowsConfiguration>().GetImageDirectory();
#endif
            _ = AppActions.SetAsync(
                new AppAction("current_info", "Check Current Weather", icon: "current_info"),
                new AppAction("add_location", "Add a Location", icon: "add_location")
            );
        }
        catch (System.Exception ex)
        {
            Debug.WriteLine("App Actions not supported", ex);
        }
    }

    private void SetupTrayIcon()
    {
        var trayService = ServiceProvider.GetService<ITrayService>();

        if (trayService != null)
        {
            trayService.Initialize();
            trayService.ClickHandler = () =>
                ServiceProvider.GetService<INotificationService>()
                    ?.ShowNotification("Hello Build! 😻 From .NET MAUI", "How's your weather?  It's sunny where we are 🌞");
        }
    }

#if PREVIEWS
    [Preview("Thunderstorms", typeof(HomePage))]
    public static ShellPreview Thunderstorms() => new("///home", nameof(HomeViewModel.Current),
        new Current
        {
            Phrase = "fluent_weather_thunderstorm_20_filled",
            Description = "Thunderstorms",
            Temperature = "72"
        });

    [Preview("Mixed weather", typeof(HomePage))]
    public static ShellPreview MixedWeather() => new("///home");
#endif
}
