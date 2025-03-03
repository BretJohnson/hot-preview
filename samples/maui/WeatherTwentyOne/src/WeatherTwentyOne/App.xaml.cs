using System.Diagnostics;
using Microsoft.UIPreview.Maui;
using Microsoft.UIPreview.Maui.Pages;
using WeatherTwentyOne.Pages;

namespace WeatherTwentyOne;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();

#if PREVIEWS
        this.EnablePreviewMode();
#endif

        /*
        if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            Shell.Current.CurrentItem = appShell.PhoneTabs;
        */
    }
}
