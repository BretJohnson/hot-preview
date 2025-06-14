using EcommerceMAUI.Model;
using EcommerceMAUI.ViewModel;
using System.Collections.ObjectModel;

namespace EcommerceMAUI.Views;

public partial class FinishCartView : ContentPage
{
	public FinishCartView(ObservableCollection<ProductListModel> products, DeliveryTypeModel deliveryType, AddressModel address, CardInfoModel card)
	{
		InitializeComponent();
		BindingContext = new FinishCartViewModel(products, deliveryType, address, card);
    }

#if PREVIEWS
    [Preview]
    public static FinishCartView Preview() => new(
		new ObservableCollection<ProductListModel>(ProductListModel.GetPreviewProducts()),
		new DeliveryTypeModel(), new AddressModel(), new CardInfoModel());
#endif
}
