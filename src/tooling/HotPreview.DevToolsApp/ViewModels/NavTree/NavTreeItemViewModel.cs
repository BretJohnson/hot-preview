using Microsoft.UI.Xaml.Data;

namespace HotPreview.DevToolsApp.ViewModels;

[Bindable]
public abstract partial class NavTreeItemViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private bool _isExpanded = false;

    public NavTreeItemViewModel()
    {
    }

    public string Content => DisplayName;

    public abstract string DisplayName { get; }
    public abstract string PathIcon { get; }
    public virtual IReadOnlyList<NavTreeItemViewModel>? Children { get; }

    /// <summary>
    /// Tooltip text for this nav tree item. Null or empty means no tooltip.
    /// </summary>
    public virtual string? ToolTipText => null;

    /// <summary>
    /// Returns children that are currently visible based on the IsVisible property.
    /// Used for filtering in search scenarios.
    /// </summary>
    public IReadOnlyList<NavTreeItemViewModel>? FilteredChildren =>
        Children?.Where(child => child.IsVisible).ToList();

    /// <summary>
    /// Notifies that FilteredChildren may have changed.
    /// Should be called when child visibility changes.
    /// </summary>
    public void NotifyFilteredChildrenChanged()
    {
        OnPropertyChanged(nameof(FilteredChildren));
    }

    /// <summary>
    /// Updates preview snapshots. Override in derived classes to implement snapshot functionality.
    /// </summary>
    public virtual Task UpdatePreviewSnapshotsAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when this item is invoked (selected) in the TreeView.
    /// Override this method in derived classes to implement navigation or other actions.
    /// </summary>
    public virtual void OnItemInvoked()
    {
    }

    /// <summary>
    /// Determines if this item or any of its children match the given search text.
    /// </summary>
    /// <param name="searchText">The search text to match against.</param>
    /// <returns>True if this item or any child matches the search text.</returns>
    public virtual bool MatchesSearch(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return true;

        // Check if this item's display name contains the search text (case-insensitive)
        if (DisplayName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            return true;

        // Check if any children match
        if (Children is not null)
        {
            foreach (NavTreeItemViewModel child in Children)
            {
                if (child.MatchesSearch(searchText))
                    return true;
            }
        }

        return false;
    }
}
