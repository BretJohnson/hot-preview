using HotPreview.DevToolsApp.ViewModels;
using HotPreview.DevToolsApp.ViewModels.NavTree;

namespace HotPreview.DevToolsApp.Views;

public sealed partial class MainPage : Page
{
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
}
