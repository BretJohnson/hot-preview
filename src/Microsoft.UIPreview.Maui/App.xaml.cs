using Microsoft.UIPreview.Maui.Pages;

namespace Microsoft.UIPreview.Maui;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();

        //MainPage = new AppShell();
        this.MainPage = new PreviewsPage();
    }
}
