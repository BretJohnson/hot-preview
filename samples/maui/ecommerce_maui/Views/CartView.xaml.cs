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
    [Preview]
    public static CartView SingleItemCart() => new(PreviewData.GetBluetoothSpeakerProducts());

    [Preview]
    public static CartView LargeCart() => new(PreviewData.GetPreviewProducts(8));
#endif
}