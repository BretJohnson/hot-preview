using Microsoft.UIPreview.Maui;
using System.Diagnostics;

namespace WeatherTwentyOne;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        //App.Current.UserAppTheme = AppTheme.Dark;

        if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            CurrentItem = PhoneTabs;

#if PREVIEWS
        this.EnablePreviewUI();
#endif

        //MainPage = new PreviewsPage();

        //Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }

    async void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
    {
        try { 
            await Shell.Current.GoToAsync($"///settings");
        }catch (Exception ex) {
            Debug.WriteLine($"err: {ex.Message}");
        }
    }
}
