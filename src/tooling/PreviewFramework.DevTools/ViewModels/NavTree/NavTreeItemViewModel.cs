using Microsoft.UI.Xaml.Data;

namespace PreviewFramework.DevTools.ViewModels;

[Bindable]
public abstract partial class NavTreeItemViewModel : ObservableObject
{
    public NavTreeItemViewModel()
    {
    }

    public string Content => DisplayName;

    public abstract string DisplayName { get; }
    public abstract string Icon { get; }
    public virtual IReadOnlyList<NavTreeItemViewModel>? Children { get; }

    /// <summary>
    /// Called when this item is invoked (selected) in the TreeView.
    /// Override this method in derived classes to implement navigation or other actions.
    /// </summary>
    public virtual void OnItemInvoked()
    {
    }
}
