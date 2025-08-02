using System.Collections.Specialized;

namespace HotPreview.DevToolsApp.Utilities;

public class BulkObservableCollection<T> : ObservableCollection<T>
{
    private bool _suppressNotification = false;

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_suppressNotification)
        {
            base.OnCollectionChanged(e);
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        _suppressNotification = true;

        foreach (T item in items)
        {
            Add(item);
        }

        _suppressNotification = false;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void ReplaceAll(IEnumerable<T> items)
    {
        _suppressNotification = true;

        Clear();
        foreach (T item in items)
        {
            Add(item);
        }

        _suppressNotification = false;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
