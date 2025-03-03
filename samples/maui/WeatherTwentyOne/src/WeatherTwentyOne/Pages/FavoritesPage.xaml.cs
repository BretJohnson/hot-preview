using Microsoft.UIPreview.Maui;
using WeatherClient2021;
using WeatherTwentyOne.ViewModels;
using Location = WeatherClient2021.Location;

namespace WeatherTwentyOne.Pages;

public partial class FavoritesPage : ContentPage
{
    public FavoritesPage()
    {
        InitializeComponent();

        BindingContext = new FavoritesViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Delay(300);
        TransitionIn();
    }

    async void TransitionIn()
    {
        foreach (var item in tiles)
        {
            await item.FadeTo(1, 800);
            await Task.Delay(50);
        }
    }

    int tileCount = 0;
    List<Frame> tiles = new List<Frame>();
    void OnHandlerChanged(object sender, EventArgs e)
    {
        Frame f = (Frame)sender;
        tiles.Add(f);
        tileCount++;
    }

#if PREVIEWS
    [Preview("Empty favorites", typeof(FavoritesPage))]
    public static ShellPreview NoFavorites() => new("///favorites", nameof(FavoritesViewModel.FavoriteLocations), new List<Location>()
        {
        });

    [Preview("Two favorites", typeof(FavoritesPage))]
    public static ShellPreview TwoFavorites() => new("///favorites", nameof(FavoritesViewModel.FavoriteLocations), new List<Location>()
        {
            new() { Name = "Redmond", Coordinate = new Coordinate(47.6740, 122.1215), Icon = "fluent_weather_cloudy_20_filled.png", WeatherStation = "USA", Value = "62°" },
            new() { Name = "St. Louis", Coordinate = new Coordinate(38.6270, 90.1994), Icon = "fluent_weather_rain_showers_night_20_filled.png", WeatherStation = "USA", Value = "74°" },
        });

    [Preview("Ten favorites", typeof(FavoritesPage))]
    public static ShellPreview TenFavorites() => new("///favorites", nameof(FavoritesViewModel.FavoriteLocations), new List<Location>()
        {
            new() { Name = "Redmond", Coordinate = new Coordinate(47.6740, 122.1215), Icon = "fluent_weather_cloudy_20_filled.png", WeatherStation = "USA", Value = "62°" },
            new() { Name = "St. Louis", Coordinate = new Coordinate(38.6270, 90.1994), Icon = "fluent_weather_rain_showers_night_20_filled.png", WeatherStation = "USA", Value = "74°" },
            new() { Name = "Boston", Coordinate = new Coordinate(42.3601, 71.0589), Icon = "fluent_weather_cloudy_20_filled.png", WeatherStation = "USA", Value = "54°" },
            new() { Name = "NYC", Coordinate = new Coordinate(40.7128, 74.0060), Icon = "fluent_weather_cloudy_20_filled.png", WeatherStation = "USA", Value = "63°" },
            new() { Name = "Amsterdam", Coordinate = new Coordinate(52.3676, 4.9041), Icon = "fluent_weather_cloudy_20_filled.png", WeatherStation = "USA", Value = "49°" },
            new() { Name = "Seoul", Coordinate = new Coordinate(37.5665, 126.9780), Icon = "fluent_weather_cloudy_20_filled.png", WeatherStation = "USA", Value = "56°" },
            new() { Name = "Johannesburg", Coordinate = new Coordinate(26.2041, 28.0473), Icon = "fluent_weather_sunny_20_filled.png", WeatherStation = "USA", Value = "62°" },
            new() { Name = "Rio", Coordinate = new Coordinate(22.9068, 43.1729), Icon = "fluent_weather_sunny_20_filled.png", WeatherStation = "USA", Value = "79°" },
            new() { Name = "Madrid", Coordinate = new Coordinate(40.4168, 3.7038), Icon = "fluent_weather_sunny_20_filled.png", WeatherStation = "USA", Value = "71°" },
            new() { Name = "Buenos Aires", Coordinate = new Coordinate(34.6037, 58.3816), Icon = "fluent_weather_sunny_20_filled.png", WeatherStation = "USA", Value = "61°" },
        });
#endif
}
