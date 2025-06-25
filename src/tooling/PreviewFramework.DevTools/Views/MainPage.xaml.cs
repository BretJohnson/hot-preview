using PreviewFramework.DevTools.ViewModels;

namespace PreviewFramework.DevTools.Views;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    public MainPageViewModel? ViewModel => DataContext as MainPageViewModel;
}
