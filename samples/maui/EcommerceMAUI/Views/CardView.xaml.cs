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
    public static CardView NoCards() => new(ExampleData.GetExampleCards(0));

    [Example("1 card")]
    public static CardView SingleCard() => new(ExampleData.GetExampleCards(1));

    [Example("2 cards")]
    public static CardView TwoCards() => new(ExampleData.GetExampleCards(2));

    [Example("6 cards")]
    public static CardView SixCards() => new(ExampleData.GetExampleCards(6));
#endif
}
