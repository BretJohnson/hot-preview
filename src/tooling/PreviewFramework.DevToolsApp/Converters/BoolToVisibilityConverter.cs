using Microsoft.UI.Xaml.Data;

namespace PreviewFramework.DevToolsApp.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool boolValue;

        // Handle different value types
        if (value is bool directBool)
        {
            boolValue = directBool;
        }
        else if (value == null)
        {
            boolValue = false;
        }
        else
        {
            // For non-null objects (like collections), consider them as true
            boolValue = true;
        }

        // Check if we should invert the result
        bool shouldInvert = parameter?.ToString()?.Equals("Invert", StringComparison.OrdinalIgnoreCase) == true;
        if (shouldInvert)
        {
            boolValue = !boolValue;
        }

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}
