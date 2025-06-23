using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PreviewFramework.Tooling;

/// <summary>
/// A base class for objects that need to notify about property changes with thread-safe UI marshaling.
/// Uses SynchronizationContext to ensure property change notifications are raised on the correct thread,
/// e.g. the UI thread.
/// </summary>
/// <param name="synchronizationContext">The SynchronizationContext to use for marshaling property change notifications to the UI thread</param>
public abstract class ToolingObservableObject(SynchronizationContext synchronizationContext) : INotifyPropertyChanged
{
    public SynchronizationContext SynchronizationContext { get; } = synchronizationContext;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event for the specified property.
    /// The event is automatically marshaled to the UI thread using the provided SynchronizationContext.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChangedEventHandler? handler = PropertyChanged;
        if (handler is null)
        {
            return;
        }

        if (SynchronizationContext == SynchronizationContext.Current)
        {
            // Already on the correct thread - create args and invoke directly
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
        else
        {
            // Marshal to the UI thread using tuple to avoid closure allocation
            SynchronizationContext.Post(state =>
            {
                (PropertyChangedEventHandler h, object sender, PropertyChangedEventArgs args) = ((PropertyChangedEventHandler, object, PropertyChangedEventArgs))state!;
                h(sender, args);
            }, (handler, this, new PropertyChangedEventArgs(propertyName)));
        }
    }

    /// <summary>
    /// Sets the property value and raises PropertyChanged if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property</typeparam>
    /// <param name="field">Reference to the backing field</param>
    /// <param name="value">The new value to set</param>
    /// <param name="propertyName">The name of the property</param>
    /// <returns>True if the property value was changed; otherwise, false</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Sets the property value using a custom equality comparer and raises PropertyChanged if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property</typeparam>
    /// <param name="field">Reference to the backing field</param>
    /// <param name="value">The new value to set</param>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="propertyName">The name of the property</param>
    /// <returns>True if the property value was changed; otherwise, false</returns>
    protected bool SetProperty<T>(ref T field, T value, IEqualityComparer<T> comparer,
        [CallerMemberName] string? propertyName = null)
    {
        if (comparer.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Sets the property value and raises PropertyChanged if the value has changed.
    /// Also raises PropertyChanged for additional dependent properties.
    /// </summary>
    /// <typeparam name="T">The type of the property</typeparam>
    /// <param name="field">Reference to the backing field</param>
    /// <param name="value">The new value to set</param>
    /// <param name="propertyName">The name of the property</param>
    /// <param name="additionalPropertyNames">Names of additional properties that should also raise PropertyChanged</param>
    /// <returns>True if the property value was changed; otherwise, false</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null, params string[] additionalPropertyNames)
    {
        if (!SetProperty(ref field, value, propertyName))
            return false;

        // Raise PropertyChanged for additional properties
        foreach (string additionalPropertyName in additionalPropertyNames)
        {
            OnPropertyChanged(additionalPropertyName);
        }

        return true;
    }

    /// <summary>
    /// Raises PropertyChanged for multiple properties.
    /// Useful when multiple properties are affected by a single operation.
    /// </summary>
    /// <param name="propertyNames">The names of the properties that changed</param>
    protected void OnPropertiesChanged(params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            OnPropertyChanged(propertyName);
        }
    }
}
