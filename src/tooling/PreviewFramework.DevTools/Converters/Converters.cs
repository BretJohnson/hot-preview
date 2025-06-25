namespace PreviewFramework.DevTools.Converters;

public static class Converters
{
    public static bool Not(bool value) => !value;

    public static bool IsNull(object value) => value is null;

    public static bool IsNotNull(object value) => value is not null;

    public static Visibility Visible(bool value) =>
        value ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility VisibleIfFalse(bool value) =>
        Visible(!value);

    public static Visibility VisibleIfNotNull(object value) =>
        Visible(IsNotNull(value));

    public static Visibility VisibleIfNull(object value) =>
        Visible(IsNull(value));
}
