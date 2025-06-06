﻿﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace ExampleFramework.DevTools.Presentation.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
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

public class ExpandCollapseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isExpanded)
        {
            return isExpanded ? "▼" : "▶";
        }
        return "▶";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class SelectedTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isSelected)
        {
            return isSelected ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255))
                              : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 55, 65, 81));
        }
        return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 55, 65, 81));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class SelectedBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isSelected)
        {
            return isSelected ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 59, 130, 246))
                              : new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
        }
        return new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
