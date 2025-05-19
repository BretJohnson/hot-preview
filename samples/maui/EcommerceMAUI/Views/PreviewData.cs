using EcommerceMAUI.Model;
using System.Collections.ObjectModel;

namespace EcommerceMAUI.Views
{
#if EXAMPLES
    public static class PreviewData
    {
        public static ObservableCollection<ProductListModel> GetBluetoothSpeakerProducts()
        {
            var products = new ObservableCollection<ProductListModel>();
            products.Add(new ProductListModel() { Name = "Smart Bluetooth Speaker", BrandName = "Google LLC", Qty = 1, Price = 900, ImageUrl = "https://raw.githubusercontent.com/exendahal/ecommerceXF/master/eCommerce/eCommerce.Android/Resources/drawable/Image3.png" });
            return products;
        }

        public static ObservableCollection<ProductListModel> GetPreviewProducts(int count)
        {
            var products = new ObservableCollection<ProductListModel>();
            products.AddIfLessThanMax(count, new ProductListModel() { Name = "BeoPlay Speaker", BrandName = "Bang and Olufsen", Qty = 1, Price = 755, ImageUrl = "https://raw.githubusercontent.com/exendahal/ecommerceXF/master/eCommerce/eCommerce.Android/Resources/drawable/Image1.png" });
            products.AddIfLessThanMax(count, new ProductListModel() { Name = "Leather Wristwatch", BrandName = "Tag Heuer", Qty = 1, Price = 450, ImageUrl = "https://raw.githubusercontent.com/exendahal/ecommerceXF/master/eCommerce/eCommerce.Android/Resources/drawable/Image2.png" });
            products.AddIfLessThanMax(count, new ProductListModel() { Name = "Smart Bluetooth Speaker", BrandName = "Google LLC", Qty = 1, Price = 900, ImageUrl = "https://raw.githubusercontent.com/exendahal/ecommerceXF/master/eCommerce/eCommerce.Android/Resources/drawable/Image3.png" });
            products.AddIfLessThanMax(count, new ProductListModel() { Name = "Smart Luggage", BrandName = "Smart Inc", Price = 1200, ImageUrl = "https://raw.githubusercontent.com/exendahal/ecommerceXF/master/eCommerce/eCommerce.Android/Resources/drawable/Image4.png" });
            products.AddIfLessThanMax(count, new ProductListModel() { Name = "Smart Bluetooth Speaker", BrandName = "Bang and Olufsen", Qty = 1, Price = 90, ImageUrl = "https://raw.githubusercontent.com/exendahal/ecommerceXF/master/eCommerce/eCommerce.Android/Resources/drawable/Image1.png" });
            products.AddIfLessThanMax(count, new ProductListModel() { Name = "B&o Desk Lamp", BrandName = "Bang and Olufsen", Qty = 1, Price = 450, ImageUrl = "https://raw.githubusercontent.com/exendahal/ecommerceXF/master/eCommerce/eCommerce.Android/Resources/drawable/Image7.png" });
            products.AddIfLessThanMax(count, new ProductListModel() { Name = "BeoPlay Stand Speaker", BrandName = "Bang and Olufse", Qty = 1, Price = 3000, ImageUrl = "https://raw.githubusercontent.com/exendahal/ecommerceXF/master/eCommerce/eCommerce.Android/Resources/drawable/Image8.png" });
            products.AddIfLessThanMax(count, new ProductListModel() { Name = "Airpods", BrandName = "B&o Phone Case", Qty = 1, Price = 30, ImageUrl = "https://raw.githubusercontent.com/exendahal/ecommerceXF/master/eCommerce/eCommerce.Android/Resources/drawable/Image9.png" });
            return products;
        }

        public static ObservableCollection<CardInfoModel> GetPreviewCards(int count)
        {
            var cards = new ObservableCollection<CardInfoModel>();
            cards.AddIfLessThanMax(count, new CardInfoModel() { CardNumber = "371449635398431", CardValidationCode = "123", ExpirationDate = "2024-12-01" });
            cards.AddIfLessThanMax(count, new CardInfoModel() { CardNumber = "38520000023237", CardValidationCode = "456", ExpirationDate = "2025-12-01" });
            cards.AddIfLessThanMax(count, new CardInfoModel() { CardNumber = "6011000990139424", CardValidationCode = "789", ExpirationDate = "2026-12-01" });
            cards.AddIfLessThanMax(count, new CardInfoModel() { CardNumber = "3566002020360505", CardValidationCode = "321", ExpirationDate = "2027-12-01" });
            cards.AddIfLessThanMax(count, new CardInfoModel() { CardNumber = "5555555555554444", CardValidationCode = "654", ExpirationDate = "2028-12-01" });
            cards.AddIfLessThanMax(count, new CardInfoModel() { CardNumber = "4012888888881881", CardValidationCode = "987", ExpirationDate = "2028-12-01" });
            return cards;
        }

        public static void AddIfLessThanMax<T>(this ObservableCollection<T> collection, int maxCount, T item)
        {
            if (collection.Count < maxCount)
            {
                collection.Add(item);
            }
        }
#endif
    }
}
