using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace ExampleFramework.Maui.Utilities;

public static class IconUtilities
{
    /// <summary>
    /// Returns the appropriate icon based on the current application theme
    /// </summary>
    /// <param name="lightIcon">The icon to use when in light theme</param>
    /// <param name="darkIcon">The icon to use when in dark theme</param>
    /// <returns>The appropriate icon for the current theme</returns>
    public static string GetIcon(string baseName)
    {
        string suffix = Application.Current?.RequestedTheme == AppTheme.Dark ? "dark" : "light";
        return $"ic_preview_{baseName}__{suffix}.png";
    }
}
