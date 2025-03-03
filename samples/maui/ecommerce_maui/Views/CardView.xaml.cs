using EcommerceMAUI.Model;
using EcommerceMAUI.ViewModel;
using System.Collections.ObjectModel;

namespace EcommerceMAUI.Views;

public partial class CardView : ContentPage
{
    public CardView(ObservableCollection<CardInfoModel>? cards = null)
	{
		InitializeComponent();
		BindingContext = new CardViewModel(cards);
    }

#if PREVIEWS
    [Preview("No Cards")]
    public static CardView NoCards() => new(PreviewData.GetPreviewCards(0));

    [Preview("Single Card")]
    public static CardView SingleCard() => new(PreviewData.GetPreviewCards(1));

    [Preview("Two Cards")]
    public static CardView TwoCards() => new(PreviewData.GetPreviewCards(2));

    [Preview("Six Cards")]
    public static CardView SixCards() => new(PreviewData.GetPreviewCards(6));
#endif
}