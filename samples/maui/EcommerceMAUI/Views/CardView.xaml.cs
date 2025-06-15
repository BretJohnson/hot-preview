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

#if PREVIEWS
    [Preview("0 cards")]
    public static CardView NoCards() => new(PreviewData.GetPreviewCards(0));

    [Preview("1 card")]
    public static CardView SingleCard() => new(PreviewData.GetPreviewCards(1));

    [Preview("2 cards")]
    public static CardView TwoCards() => new(PreviewData.GetPreviewCards(2));

    [Preview("6 cards")]
    public static CardView SixCards() => new(PreviewData.GetPreviewCards(6));
#endif
}
