using HotPreview.DevToolsApp.Utilities;
using HotPreview.SharedModel;
using Microsoft.UI.Xaml.Data;

namespace HotPreview.DevToolsApp.ViewModels;

[Bindable]
public abstract partial class NavTreeItemViewModel : ObservableObject
{
    public NavTreeItemViewModel()
    {
    }

    public string Content => DisplayName;

    public abstract string DisplayName { get; }
    public abstract string PathIcon { get; }
    public virtual IReadOnlyList<NavTreeItemViewModel>? Children { get; }

    /// <summary>
    /// Command for updating snapshots. Override in derived classes to implement snapshot functionality.
    /// </summary>
    public virtual ICommand? UpdateSnapshotsCommand { get; }

    /// <summary>
    /// Called when this item is invoked (selected) in the TreeView.
    /// Override this method in derived classes to implement navigation or other actions.
    /// </summary>
    public virtual void OnItemInvoked()
    {
    }
}
