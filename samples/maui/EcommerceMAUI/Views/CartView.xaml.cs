using System.Collections.ObjectModel;
using EcommerceMAUI.Model;
using EcommerceMAUI.ViewModel;

namespace EcommerceMAUI.Views;

public partial class CartView : ContentPage
{
    public CartView(ObservableCollection<ProductListModel> products = null)
    {
        InitializeComponent();
        BindingContext = new CartViewModel(products);
    }

#if EXAMPLES
    [Example("empty")]
    public static CartView SingleItemCart() => new(PreviewData.GetBluetoothSpeakerProducts());

    [Example("3 items")]
    public static CartView MediumCart() => new(PreviewData.GetPreviewProducts(3));

    [Example("8 items")]
    public static CartView LargeCart() => new(PreviewData.GetPreviewProducts(8));
#endif
}
