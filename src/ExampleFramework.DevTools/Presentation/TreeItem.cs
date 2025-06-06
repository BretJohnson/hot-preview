namespace ExampleFramework.DevTools.Presentation.Models;

public abstract partial class TreeItem : ObservableObject
{
    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isSelected;

    public abstract string DisplayName { get; }
    public abstract string Icon { get; }
    public virtual ObservableCollection<TreeItem>? Children { get; }
    public virtual bool HasChildren => Children?.Count > 0;
    public virtual bool IsExpandable => HasChildren;
}
