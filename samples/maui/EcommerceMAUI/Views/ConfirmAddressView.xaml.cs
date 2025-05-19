using EcommerceMAUI.Model;
using EcommerceMAUI.ViewModel;
using System.Collections.ObjectModel;

namespace EcommerceMAUI.Views;

public partial class ConfirmAddressView : ContentPage
{
    public ConfirmAddressView(ObservableCollection<ProductListModel> products, DeliveryTypeModel deliveryType, AddressModel? primaryAddress = null)
    {
        InitializeComponent();
        BindingContext = new ConfirmAddressViewModel(products, deliveryType, primaryAddress);
    }

#if EXAMPLES
    [Example]
    public static ConfirmAddressView Example() => new(PreviewData.GetPreviewProducts(1), new DeliveryTypeModel(),
        new AddressModel()
        {
            StreetOne = "21, Alex Davidson Avenue",
            StreetTwo = "Opposite Omegatron, Vicent Quarters",
            City = "Victoria Island",
            State = "Lagos State"
        });
#endif
}