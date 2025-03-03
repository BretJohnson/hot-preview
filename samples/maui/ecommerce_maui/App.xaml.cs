using EcommerceMAUI.Views;
using Microsoft.UIPreview.Maui;

namespace EcommerceMAUI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Current.UserAppTheme = AppTheme.Light;
        MainPage = new LoginView();

#if PREVIEWS
        this.EnablePreviewMode();
#endif
    }
}
