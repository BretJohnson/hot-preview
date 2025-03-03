using System.Collections.ObjectModel;
using EcommerceMAUI.Model;
using EcommerceMAUI.ViewModel;

namespace EcommerceMAUI.Views;

public partial class CartView : ContentPage
{
    public CartView(ObservableCollection<ProductListModel>? products = null)
    {
        InitializeComponent();
        BindingContext = new CartViewModel(products);
    }

#if PREVIEWS
    [Preview("Single Item Cart")]
    public static CartView BluetoothSpeaker() => new(PreviewData.GetBluetoothSpeakerProducts());

    [Preview("Large Cart")]
    public static CartView LargeCart() => new(PreviewData.GetPreviewProducts(8));
#endif
}