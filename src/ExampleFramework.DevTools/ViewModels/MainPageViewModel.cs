using ExampleFramework.DevTools.ViewModels;
using ExampleFramework.DevTools.ViewModels.NavTree;
using Microsoft.UI.Xaml.Data;

namespace ExampleFramework.DevTools.Presentation;

[Bindable]
public partial class MainPageViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<NavTreeItemViewModel> _navTreeItems = [];

    public MainPageViewModel(IOptions<AppConfig> appInfo, INavigator navigator)
    {
        DevToolsManager.Instance.MainPageViewModel = this;

        _navigator = navigator;
        Title = "Examples";

        // Initialize commands
        PlayCommand = new RelayCommand(Play);
        SettingsCommand = new RelayCommand(Settings);

        // Initialize sample data
        InitializeNavTreeItems();
    }

    public string Title { get; }

    public ICommand PlayCommand { get; }

    public ICommand SettingsCommand { get; }

    private void InitializeNavTreeItems()
    {
        NavTreeItems.Clear();

        // Introduction item
        NavTreeItems.Add(new UIComponentItemViewModel("Introduction", "📄"));

        // APPLICATION section
        var applicationSection = new SectionItemViewModel("APPLICATION", "");
        applicationSection.IsExpanded = true;

        // ProductCard
        applicationSection.Children.Add(new UIComponentItemViewModel("ProductCard", "🧩"));

        // Documentation with sub-items
        var documentationItem = new UIComponentItemViewModel("Documentation", "📋", new ObservableCollection<NavTreeItemViewModel>
        {
            new ExampleItemViewModel("Default", "📄"),
            new ExampleItemViewModel("Expanded", "📄"),
            new ExampleItemViewModel("Added to cart", "📄")
        });
        documentationItem.IsExpanded = true;
        documentationItem.IsSelected = true; // This matches the blue highlight in screenshot
        applicationSection.Children.Add(documentationItem);

        // Other application items
        applicationSection.Children.Add(new UIComponentItemViewModel("Dashboard", "🧩"));
        applicationSection.Children.Add(new UIComponentItemViewModel("Homepage", "🧩"));
        applicationSection.Children.Add(new UIComponentItemViewModel("User Profile", "🧩"));
        applicationSection.Children.Add(new UIComponentItemViewModel("Sign In", "🧩"));

        NavTreeItems.Add(applicationSection);

        // DESIGN SYSTEM section
        var designSystemSection = new SectionItemViewModel("DESIGN SYSTEM", "");
        designSystemSection.IsExpanded = true;

        designSystemSection.Children.Add(new UIComponentItemViewModel("ActivityList", "📁"));
        designSystemSection.Children.Add(new UIComponentItemViewModel("Form", "☐"));
        designSystemSection.Children.Add(new UIComponentItemViewModel("Avatar", "🧩"));
        designSystemSection.Children.Add(new UIComponentItemViewModel("Button", "🧩"));
        designSystemSection.Children.Add(new UIComponentItemViewModel("Footer", "🧩"));
        designSystemSection.Children.Add(new UIComponentItemViewModel("Header", "🧩"));
        designSystemSection.Children.Add(new UIComponentItemViewModel("Pagination", "🧩"));

        NavTreeItems.Add(designSystemSection);
    }

    private void Play()
    {
        // TODO: Implement play functionality
    }

    private void Settings()
    {
        // TODO: Implement settings functionality
    }

    partial void OnSearchTextChanged(string value)
    {
        // TODO: Implement search filtering
    }
}
