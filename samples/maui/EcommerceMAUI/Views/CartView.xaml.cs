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

#if PREVIEWS
    [Preview("empty")]
    public static CartView SingleItemCart() => new(PreviewData.GetBluetoothSpeakerProducts());

    [Preview("3 items")]
    public static CartView MediumCart() => new(PreviewData.GetPreviewProducts(3));

    [Preview("8 items")]
    public static CartView LargeCart() => new(PreviewData.GetPreviewProducts(8));
#endif
}
