using System.Reflection;
using HotPreview.DevToolsApp.ViewModels;
using HotPreview.DevToolsApp.ViewModels.NavTree;
using HotPreview.Tooling;
using Microsoft.UI.Xaml.Input;

namespace HotPreview.DevToolsApp.Views;

public sealed partial class MainPage : Page
{
    private MenuFlyout? _settingsFlyout;
    private ToggleMenuFlyoutItem? _bringToFrontToggleItem;
    public MainPage()
    {
        this.InitializeComponent();
    }

    public MainPageViewModel? ViewModel => DataContext as MainPageViewModel;

    private void OnNavTreeItemInvoked(TreeView treeView, TreeViewItemInvokedEventArgs args)
    {
        if (args.InvokedItem is NavTreeItemViewModel selectedItem)
        {
            // If this is a UIComponentViewModel with children, expand and select the first child.
            // OnItemInvoked is then called for the selection to navigate to that preview.
            if (selectedItem is UIComponentViewModel uiComponentViewModel &&
                uiComponentViewModel.Children?.Count > 0)
            {
                TreeViewItem? selectedTreeViewItem = treeView.ContainerFromItem(args.InvokedItem) as TreeViewItem;
                if (selectedTreeViewItem is not null)
                {
                    TreeViewNode? selectedNode = treeView.NodeFromContainer(selectedTreeViewItem);

                    if (selectedNode is not null)
                    {
                        // Expand the node if it's collapsed
                        if (!selectedNode.IsExpanded)
                        {
                            selectedNode.IsExpanded = true;
                        }

                        // Select the first child and invoke it
                        if (selectedNode.Children.Count > 0)
                        {
                            TreeViewNode firstChildNode = selectedNode.Children[0];

                            // Clear current selection and set new selection with delay
                            treeView.SelectedNode = null;
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                treeView.SelectedNode = firstChildNode;
                            });

                            NavTreeItemViewModel firstChildItem = uiComponentViewModel.Children[0];
                            firstChildItem.OnItemInvoked();
                            return;
                        }
                    }
                }
            }

            selectedItem.OnItemInvoked();
        }
    }

    private void OnTreeViewItemRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        // TODO: Potentially give visual feedback here, in some better way than what's below
#if false
        if (sender is TreeViewItem treeViewItem)
        {
            // Prevent the right-click from changing the selection
            e.Handled = true;

            // Find the Border element to apply visual feedback
            if (FindChildByName(treeViewItem, "ItemContentBorder") is Border border)
            {
                // Apply visual feedback by temporarily changing the background
                Brush originalBrush = border.Background;
                border.Background = Application.Current.Resources["SubtleFillColorSecondaryBrush"] as Brush;

                // Reset the background after a short delay
                DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                {
                    border.Background = originalBrush;
                });
            }
        }
#endif
    }

    private static FrameworkElement? FindChildByName(DependencyObject parent, string name)
    {
        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is FrameworkElement element && element.Name == name)
            {
                return element;
            }

            FrameworkElement? result = FindChildByName(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe)
        {
            return;
        }

        if (_settingsFlyout is null)
        {
            _settingsFlyout = new MenuFlyout();

            _bringToFrontToggleItem = new ToggleMenuFlyoutItem
            {
                Text = "Bring app to front on navigate",
                IsChecked = Settings.BringAppToFrontOnNavigate
            };
            _bringToFrontToggleItem.Click += OnBringToFrontToggleClick;
            _settingsFlyout.Items.Add(_bringToFrontToggleItem);

            _settingsFlyout.Items.Add(new MenuFlyoutSeparator());

            var aboutItem = new MenuFlyoutItem
            {
                Text = "About"
            };
            aboutItem.Click += OnAboutMenuItemClick;
            _settingsFlyout.Items.Add(aboutItem);
        }

        _settingsFlyout.ShowAt(fe);
    }

    private async void OnAboutMenuItemClick(object sender, RoutedEventArgs e)
    {
        string version = GetVersionString();
        var dialog = new ContentDialog
        {
            Title = "Hot Preview DevTools",
            Content = $"Version {version}",
            PrimaryButtonText = "OK",
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
    }

    private void OnBringToFrontToggleClick(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleMenuFlyoutItem toggle)
        {
            Settings.BringAppToFrontOnNavigate = toggle.IsChecked;
        }
    }

    private static string GetVersionString()
    {
        try
        {
            Assembly asm = typeof(MainPage).Assembly;
            AssemblyInformationalVersionAttribute? info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (info is not null && !string.IsNullOrWhiteSpace(info.InformationalVersion))
            {
                return info.InformationalVersion;
            }
            return asm.GetName().Version?.ToString() ?? "unknown";
        }
        catch
        {
            return "unknown";
        }
    }

    private async void OnUpdateSnapshotsClicked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem menuItem &&
            menuItem.DataContext is NavTreeItemViewModel navTreeItem)
        {
            // Get the current selection directly from the TreeView
            NavTreeItemViewModel? currentSelection = NavTree.SelectedItem as NavTreeItemViewModel;

            // Execute the snapshot update using the async method
            await navTreeItem.UpdatePreviewSnapshotsAsync();

            // Navigate back to the current selection if it exists
            if (currentSelection is not null && ViewModel?.CurrentApp is not null)
            {
                if (currentSelection is PreviewViewModel previewViewModel)
                {
                    ViewModel.CurrentApp.NavigateToPreview(previewViewModel.UIComponent, previewViewModel.Preview);
                }
                else if (currentSelection is UIComponentViewModel componentViewModel &&
                         componentViewModel.UIComponent.HasSinglePreview)
                {
                    ViewModel.CurrentApp.NavigateToPreview(componentViewModel.UIComponent, componentViewModel.UIComponent.DefaultPreview);
                }
            }
        }
    }
}
