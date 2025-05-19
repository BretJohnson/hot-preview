using EcommerceMAUI.ViewModel;
using System.Collections.ObjectModel;

namespace EcommerceMAUI.Model
{
    public class ProductListModel: BaseViewModel
    {
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public string BrandName { get; set; }
        public double Price { get; set; }
        public string Details { get; set; }
        public double Qty { get; set; } = 1;

        private bool _IsAvailable;
        public bool IsAvailable
        {
            get => _IsAvailable;
            set
            {
                if (_IsAvailable != value)
                {
                    _IsAvailable = value;
                    OnPropertyChanged(nameof(IsAvailable));
                    OnPropertyChanged(nameof(AvailableColor));
                }
            }
        }
        public Color AvailableColor
        {
            get
            {
                if (IsAvailable)
                {
                    return Color.FromArgb("#00C569");
                }
                return Color.FromArgb("#FFB900");
            }
        }

        public string AvailableText
        {
            get
            {
                if (IsAvailable)
                {
                    return "In Stock";
                }
                return "Out of Stock";
            }
        }

#if EXAMPLES
        public static ObservableCollection<ProductListModel> GetExampleProducts()
        {
            return new ObservableCollection<ProductListModel>
            {
                new ProductListModel
                {
                    ImageUrl = "https://example.com/image1.jpg",
                    Name = "Product 1",
                    BrandName = "Brand A",
                    Price = 29.99,
                    Details = "Details about Product 1",
                    Qty = 10,
                    IsAvailable = true
                },
                new ProductListModel
                {
                    ImageUrl = "https://example.com/image2.jpg",
                    Name = "Product 2",
                    BrandName = "Brand B",
                    Price = 49.99,
                    Details = "Details about Product 2",
                    Qty = 5,
                    IsAvailable = false
                },
                new ProductListModel
                {
                    ImageUrl = "https://example.com/image3.jpg",
                    Name = "Product 3",
                    BrandName = "Brand C",
                    Price = 19.99,
                    Details = "Details about Product 3",
                    Qty = 20,
                    IsAvailable = true
                }
            };
        }
#endif
    }
}
