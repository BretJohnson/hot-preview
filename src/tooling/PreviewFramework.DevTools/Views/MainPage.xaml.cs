using Microsoft.UI.Xaml.Controls;
using PreviewFramework.DevTools.ViewModels;

namespace PreviewFramework.DevTools.Views;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    public MainPageViewModel? ViewModel => DataContext as MainPageViewModel;

    private void OnNavTreeItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is NavTreeItemViewModel selectedItem)
        {
            selectedItem.OnItemInvoked();
        }
    }
}
