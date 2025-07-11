using HotPreview.App.Maui.ViewModels;
using Microsoft.Maui.Controls;

namespace HotPreview.App.Maui.Pages;

public partial class PreviewsPage : ContentPage
{
    public PreviewsPage()
    {
        InitializeComponent();
        BindingContext = PreviewsViewModel.Instance;
    }


}
