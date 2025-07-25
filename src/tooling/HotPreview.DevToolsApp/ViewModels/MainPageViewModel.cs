using HotPreview.DevToolsApp.Utilities;
using HotPreview.DevToolsApp.ViewModels.NavTree;
using HotPreview.Tooling;
using HotPreview.Tooling.Services;
using Microsoft.UI.Xaml.Data;

namespace HotPreview.DevToolsApp.ViewModels;

[Bindable]
public partial class MainPageViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly StatusReporter _statusReporter;
    private readonly DevToolsManager _devToolsManager;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private AppManager? _currentApp;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    /// <summary>
    /// Returns true when there is a connected app (CurrentApp is not null).
    /// </summary>
    public bool HaveApp => CurrentApp is not null;

    public string PageTitle => CurrentApp is not null ? CurrentApp.ProjectName : "Hot Preview";

    public MainPageViewModel(IOptions<AppConfig> appInfo, INavigator navigator, StatusReporter statusReporter, DevToolsManager devToolsManager)
    {
        _devToolsManager = devToolsManager;
        _devToolsManager.MainPageViewModel = this;

        _navigator = navigator;
        _statusReporter = statusReporter;

        // Subscribe to status updates
        _statusReporter.StatusChanged += OnStatusChanged;

        // Initialize commands
        PlayCommand = new RelayCommand(Play);
        SettingsCommand = new RelayCommand(Settings);

        // Initialize CurrentApp with the first app, if there is one. Update CurrentApp when AppsManager.Apps changes.
        UpdateCurrentAppFromAppsManager();
        _devToolsManager.AppsManager.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(AppsManager.Apps))
            {
                UpdateCurrentAppFromAppsManager();
            }
        };

        // Initialize sample data
        UpdateNavTreeItems();

        // Set initial status message
        string initialStatus = HaveApp ? $"Connected to {CurrentApp!.ProjectName}" : "No app connected";
        _statusReporter.UpdateStatus(initialStatus);
    }

    public BulkObservableCollection<NavTreeItemViewModel> NavTreeItems { get; } = [];

    public ICommand PlayCommand { get; }

    public ICommand SettingsCommand { get; }

    private void UpdateCurrentAppFromAppsManager()
    {
        IEnumerable<AppManager> apps = _devToolsManager.AppsManager.Apps;

        // If current app is still in the list, keep it
        if (CurrentApp is not null && apps.Contains(CurrentApp))
        {
            return;
        }

        // Otherwise, select the first app or null if list is empty
        AppManager? newApp = apps.FirstOrDefault();

        if (newApp != null && CurrentApp == null)
        {
            _statusReporter.UpdateStatus($"Connected to {newApp.ProjectName}");
        }
        else if (newApp == null && CurrentApp != null)
        {
            _statusReporter.UpdateStatus("App disconnected");
        }

        CurrentApp = newApp;

        if (CurrentApp == null)
        {
            _statusReporter.UpdateStatus("No app connected");
        }
    }

    partial void OnCurrentAppChanged(AppManager? oldValue, AppManager? newValue)
    {
        if (oldValue is not null)
        {
            oldValue.PropertyChanged -= OnCurrentAppPropertyChanged;
        }

        if (newValue is not null)
        {
            newValue.PropertyChanged += OnCurrentAppPropertyChanged;
        }

        // Notify that dependent properties have changed
        OnPropertyChanged(nameof(HaveApp));
        OnPropertyChanged(nameof(PageTitle));

        UpdateNavTreeItems();
    }

    private void OnCurrentAppPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppManager.PreviewsManager))
        {
            UpdateNavTreeItems();
        }
    }

    private void UpdateNavTreeItems()
    {
        PreviewsManagerTooling? previewsManager = CurrentApp?.PreviewsManager;
        if (previewsManager is not null)
        {
            List<NavTreeItemViewModel> newNavTreeItems = [];
            foreach (var (category, components) in previewsManager.CategorizedUIComponents)
            {
                if (components.Count > 0)
                {
                    var sectionViewModel = new SectionItemViewModel(category.Name.ToUpperInvariant());
                    foreach (UIComponentTooling uiComponent in components)
                    {
                        sectionViewModel.AddChild(new UIComponentViewModel(this, uiComponent));
                    }
                    newNavTreeItems.Add(sectionViewModel);
                }
            }

            // Add Commands section if there are any commands
            var commands = previewsManager.Commands.OrderBy(cmd => cmd.DisplayName).ToList();
            if (commands.Count > 0)
            {
                var commandsSection = new SectionItemViewModel("COMMANDS");
                foreach (PreviewCommandTooling command in commands)
                {
                    commandsSection.AddChild(new CommandViewModel(this, command));
                }
                newNavTreeItems.Add(commandsSection);
            }

            NavTreeItems.ReplaceAll(newNavTreeItems);
        }
        else
        {
            NavTreeItems.Clear();
        }
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
        _statusReporter.UpdateStatus("Starting project...");

        bool success = _devToolsManager.Run();

        if (success)
        {
            _statusReporter.UpdateStatus("Project started successfully");
        }
        else
        {
            _statusReporter.UpdateStatus("Failed to start project");
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

    /// <summary>
    /// Updates the status message displayed in the status bar.
    /// </summary>
    /// <param name="message">The status message to display.</param>
    public void UpdateStatusMessage(string message)
    {
        StatusMessage = message;
    }

    private void OnStatusChanged(object? sender, string message)
    {
        StatusMessage = message;
    }
}
