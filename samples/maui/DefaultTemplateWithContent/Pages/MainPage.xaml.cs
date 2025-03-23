using DefaultTemplateWithContent.Models;
using DefaultTemplateWithContent.PageModels;

namespace DefaultTemplateWithContent.Pages;
public partial class MainPage : ContentPage
{
    public MainPage(MainPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}