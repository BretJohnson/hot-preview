namespace PreviewFramework.DevTools.Converters;

public static class Converters
{
    // Boolean operations
    public static bool Not(bool value) => !value;
    public static bool And(bool a, bool b) => a && b;
    public static bool Or(bool a, bool b) => a || b;

    // Null checks
    public static bool IsNull(object value) => value is null;
    public static bool IsNotNull(object value) => value is not null;
    public static bool IsNullOrEmpty(string value) => string.IsNullOrEmpty(value);

    // Visibility
    public static Visibility ToVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;
    public static Visibility ToVisibilityInverted(bool value) => !value ? Visibility.Visible : Visibility.Collapsed;
    public static Visibility VisibleWhenTrue(bool value) => ToVisibility(value);
    public static Visibility VisibleWhenFalse(bool value) => ToVisibilityInverted(value);
    public static Visibility VisibleWhenNotNull(object value) => ToVisibility(value is not null);
    public static Visibility VisibleWhenNull(object value) => ToVisibility(value is null);

    // String operations
    public static string ToUpper(string value) => value?.ToUpperInvariant() ?? string.Empty;
    public static string ToLower(string value) => value?.ToLowerInvariant() ?? string.Empty;
    public static string Format(string format, object arg) => string.Format(format, arg);
}
