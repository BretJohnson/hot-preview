using EcommerceMAUI.Views;
using Microsoft.UIPreview.App;
using Microsoft.UIPreview.Maui;

namespace EcommerceMAUI;

public partial class App : Application
{
    public App()
    {
        var inst = MauiPreviewApplication.Instance;
        var inst2 = PreviewApplicationRetriever.GetPreviewAppService();


        InitializeComponent();
        Current.UserAppTheme = AppTheme.Light;
        MainPage = new LoginView();
    }
}
