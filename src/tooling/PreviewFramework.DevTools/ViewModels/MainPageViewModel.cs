using PreviewFramework.DevTools.ViewModels.NavTree;
using PreviewFramework.Tooling;
using Microsoft.UI.Xaml.Data;

namespace PreviewFramework.DevTools.ViewModels;

[Bindable]
public partial class MainPageViewModel : ObservableObject
{
    private readonly INavigator _navigator;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<NavTreeItemViewModel>? _navTreeItems = [];

    [ObservableProperty]
    private AppManager? _currentApp;

    public MainPageViewModel(IOptions<AppConfig> appInfo, INavigator navigator)
    {
        DevToolsManager.Instance.MainPageViewModel = this;

        _navigator = navigator;

        // Initialize commands
        PlayCommand = new RelayCommand(Play);
        SettingsCommand = new RelayCommand(Settings);

        // Subscribe to AppsManager property changes to handle app selection
        DevToolsManager.Instance.AppsManager.PropertyChanged += OnAppsManagerPropertyChanged;

        // Initialize CurrentApp with the first available app
        UpdateCurrentAppFromAppsManager();

        // Initialize sample data
        UpdateNavTreeItems();
    }

    public ICommand PlayCommand { get; }

    public ICommand SettingsCommand { get; }

    private void OnAppsManagerPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppsManager.Apps))
        {
            UpdateCurrentAppFromAppsManager();
        }
    }

    private void OnCurrentAppPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppManager.UIComponentsManager))
        {
            UpdateNavTreeItems();
        }
    }

    private void UpdateCurrentAppFromAppsManager()
    {
        IEnumerable<AppManager> apps = DevToolsManager.Instance.AppsManager.Apps;

        // If current app is still in the list, keep it
        if (CurrentApp is not null && apps.Contains(CurrentApp))
        {
            return;
        }

        // Otherwise, select the first app or null if list is empty
        CurrentApp = apps.FirstOrDefault();
    }

    private void UpdateNavTreeItems()
    {
        UIComponentsManagerTooling? uiComponentsManager = CurrentApp?.UIComponentsManager;
        if (uiComponentsManager is not null)
        {
            List<NavTreeItemViewModel> newNavTreeItems = [];
            foreach (UIComponentTooling uiComponent in uiComponentsManager.SortedUIComponents)
            {
                newNavTreeItems.Add(new UIComponentViewModel(uiComponent));
            }

            NavTreeItems = newNavTreeItems;
        }
        else
        {
            NavTreeItems = null;
        }
    }

    partial void OnCurrentAppChanged(AppManager? oldValue, AppManager? newValue)
    {
        // Unsubscribe from the old app's property changes
        if (oldValue is not null)
        {
            oldValue.PropertyChanged -= OnCurrentAppPropertyChanged;
        }

        // Subscribe to the new app's property changes
        if (newValue is not null)
        {
            newValue.PropertyChanged += OnCurrentAppPropertyChanged;
        }

        // Update nav tree items when current app changes
        UpdateNavTreeItems();
    }

#if LATER
    private void InitializeNavTreeItemsWithSampleData()
    {
        NavTreeItems.Clear();

        // Introduction item
        NavTreeItems.Add(new UIComponentViewModel("Introduction", "📄"));

        // APPLICATION section
        var applicationSection = new SectionItemViewModel("APPLICATION", "");
        applicationSection.IsExpanded = true;

        // ProductCard
        applicationSection.Children.Add(new UIComponentViewModel("ProductCard", "🧩"));

        // Documentation with sub-items
        var documentationItem = new UIComponentViewModel("Documentation", "📋", new ObservableCollection<NavTreeItemViewModel>
        {
            new PreviewViewModel("Default", "📄"),
            new PreviewViewModel("Expanded", "📄"),
            new PreviewViewModel("Added to cart", "📄")
        });
        documentationItem.IsExpanded = true;
        documentationItem.IsSelected = true; // This matches the blue highlight in screenshot
        applicationSection.Children.Add(documentationItem);

        // Other application items
        applicationSection.Children.Add(new UIComponentViewModel("Dashboard", "🧩"));
        applicationSection.Children.Add(new UIComponentViewModel("Homepage", "🧩"));
        applicationSection.Children.Add(new UIComponentViewModel("User Profile", "🧩"));
        applicationSection.Children.Add(new UIComponentViewModel("Sign In", "🧩"));

        NavTreeItems.Add(applicationSection);

        // DESIGN SYSTEM section
        var designSystemSection = new SectionItemViewModel("DESIGN SYSTEM", "");
        designSystemSection.IsExpanded = true;

        designSystemSection.Children.Add(new UIComponentViewModel("ActivityList", "📁"));
        designSystemSection.Children.Add(new UIComponentViewModel("Form", "☐"));
        designSystemSection.Children.Add(new UIComponentViewModel("Avatar", "🧩"));
        designSystemSection.Children.Add(new UIComponentViewModel("Button", "🧩"));
        designSystemSection.Children.Add(new UIComponentViewModel("Footer", "🧩"));
        designSystemSection.Children.Add(new UIComponentViewModel("Header", "🧩"));
        designSystemSection.Children.Add(new UIComponentViewModel("Pagination", "🧩"));

        NavTreeItems.Add(designSystemSection);
    }
#endif

    private void Play()
    {
        bool success = DevToolsManager.Instance.Run();

        if (!success)
        {
            // TODO: Show error message to user (could use a notification service or dialog)
            // For now, this will be logged by the DevToolsManager
        }
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
