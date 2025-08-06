namespace HotPreview;

/// <summary>
/// Provides utility methods for working with names and type names.
/// </summary>
internal class NameUtilities
{
    public static string GetUnqualifiedName(string name)
    {
        int index = name.LastIndexOf('.');
        return index >= 0 ? name.Substring(index + 1) : name;
    }
}
