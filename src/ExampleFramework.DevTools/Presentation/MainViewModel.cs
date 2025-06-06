using System.Collections.ObjectModel;
using ExampleFramework.DevTools.Presentation.Models;

namespace ExampleFramework.DevTools.Presentation;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TreeItem> _treeItems = [];

    [ObservableProperty]
    private TreeItem? _selectedItem;

    public MainViewModel(
        IOptions<AppConfig> appInfo,
        INavigator navigator)
    {
        _navigator = navigator;
        Title = "Storybook";

        // Initialize commands
        ToggleExpandCommand = new RelayCommand<TreeItem>(ToggleExpand);
        SelectItemCommand = new RelayCommand<TreeItem>(SelectItem);
        AddComponentCommand = new RelayCommand(AddComponent);
        PlayCommand = new RelayCommand(Play);
        SettingsCommand = new RelayCommand(Settings);

        // Initialize sample data
        InitializeTreeItems();
    }

    public string Title { get; }

    public ICommand ToggleExpandCommand { get; }
    public ICommand SelectItemCommand { get; }
    public ICommand AddComponentCommand { get; }
    public ICommand PlayCommand { get; }
    public ICommand SettingsCommand { get; }

    private void InitializeTreeItems()
    {
        TreeItems.Clear();

        // Introduction item
        TreeItems.Add(new UIComponentItem("Introduction", "📄"));

        // APPLICATION section
        var applicationSection = new SectionItem("APPLICATION", "");
        applicationSection.IsExpanded = true;

        // ProductCard
        applicationSection.Children.Add(new UIComponentItem("ProductCard", "🧩"));

        // Documentation with sub-items
        var documentationItem = new UIComponentItem("Documentation", "📋", new ObservableCollection<TreeItem>
        {
            new ExampleItem("Default", "📄"),
            new ExampleItem("Expanded", "📄"),
            new ExampleItem("Added to cart", "📄")
        });
        documentationItem.IsExpanded = true;
        documentationItem.IsSelected = true; // This matches the blue highlight in screenshot
        applicationSection.Children.Add(documentationItem);

        // Other application items
        applicationSection.Children.Add(new UIComponentItem("Dashboard", "🧩"));
        applicationSection.Children.Add(new UIComponentItem("Homepage", "🧩"));
        applicationSection.Children.Add(new UIComponentItem("User Profile", "🧩"));
        applicationSection.Children.Add(new UIComponentItem("Sign In", "🧩"));

        TreeItems.Add(applicationSection);

        // DESIGN SYSTEM section
        var designSystemSection = new SectionItem("DESIGN SYSTEM", "");
        designSystemSection.IsExpanded = true;

        designSystemSection.Children.Add(new UIComponentItem("ActivityList", "📁"));
        designSystemSection.Children.Add(new UIComponentItem("Form", "☐"));
        designSystemSection.Children.Add(new UIComponentItem("Avatar", "🧩"));
        designSystemSection.Children.Add(new UIComponentItem("Button", "🧩"));
        designSystemSection.Children.Add(new UIComponentItem("Footer", "🧩"));
        designSystemSection.Children.Add(new UIComponentItem("Header", "🧩"));
        designSystemSection.Children.Add(new UIComponentItem("Pagination", "🧩"));

        TreeItems.Add(designSystemSection);
    }

    private void ToggleExpand(TreeItem? item)
    {
        if (item?.IsExpandable == true)
        {
            item.IsExpanded = !item.IsExpanded;
        }
    }

    private void SelectItem(TreeItem? item)
    {
        if (SelectedItem != null)
        {
            SelectedItem.IsSelected = false;
        }

        if (item != null)
        {
            item.IsSelected = true;
            SelectedItem = item;
        }
    }

    private void AddComponent()
    {
        // TODO: Implement add component functionality
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
