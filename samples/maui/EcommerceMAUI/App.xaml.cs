using EcommerceMAUI.Views;
using Microsoft.UIPreview.App;
using Microsoft.UIPreview.Maui;

namespace EcommerceMAUI;

public partial class App : Application
{
    public App()
    {
#if PREVIEWS
        MauiPreviewApplication.EnsureInitialized();
#endif

        InitializeComponent();
        Current.UserAppTheme = AppTheme.Light;
        MainPage = new LoginView();
    }
}
