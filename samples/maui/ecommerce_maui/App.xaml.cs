using EcommerceMAUI.Views;
using Microsoft.UIPreview.Maui;

namespace EcommerceMAUI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Current.UserAppTheme = AppTheme.Light;
        //MainPage = new LoginView();
        AppShell shell = new AppShell();
        MainPage = shell;

#if PREVIEWS
        this.EnablePreviewMode();
#endif

        // Register for Shell navigation events globally
        shell.Navigated += Shell_Navigated;
    }

    private void Shell_Navigated(object sender, ShellNavigatedEventArgs e)
    {
        // This fires when any page navigation completes
        Console.WriteLine($"Navigated to: {e.Current.Location}");

        // Get reference to the displayed page
        var currentPage = Shell.Current.CurrentPage;

        // Do something with the current page
    }
}
