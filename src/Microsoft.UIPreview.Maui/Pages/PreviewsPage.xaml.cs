using Microsoft.UIPreview.Maui.ViewModels;

namespace Microsoft.UIPreview.Maui.Pages;

public partial class PreviewsPage : ContentPage
{
	public PreviewsPage()
	{
		this.InitializeComponent();
        this.BindingContext = PreviewsViewModel.Instance;
    }
}
