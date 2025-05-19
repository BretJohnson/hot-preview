using EcommerceMAUI.Model;
using EcommerceMAUI.ViewModel;
using System.Collections.ObjectModel;

namespace EcommerceMAUI.Views;

public partial class CardView : ContentPage
{
    public CardView(ObservableCollection<CardInfoModel> cards = null)
	{
		InitializeComponent();
		BindingContext = new CardViewModel(cards);
    }

#if EXAMPLES
    [Example("0 cards")]
    public static CardView NoCards() => new(PreviewData.GetPreviewCards(0));

    [Example("1 card")]
    public static CardView SingleCard() => new(PreviewData.GetPreviewCards(1));

    [Example("2 cards")]
    public static CardView TwoCards() => new(PreviewData.GetPreviewCards(2));

    [Example("6 cards")]
    public static CardView SixCards() => new(PreviewData.GetPreviewCards(6));
#endif
}
