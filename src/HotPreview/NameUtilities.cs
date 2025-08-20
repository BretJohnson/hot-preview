namespace HotPreview;

/// <summary>
/// Provides utility methods for working with names and type names.
/// </summary>
internal class NameUtilities
{
    /// <summary>
    /// Gets the simple name from a qualified name by removing namespace or type prefixes.
    /// </summary>
    /// <param name="name">The qualified name to simplify.</param>
    /// <returns>The simple name without any namespace or type prefixes.</returns>
    public static string GetSimpleName(string name)
    {
        int index = name.LastIndexOf('.');
        return index >= 0 ? name.Substring(index + 1) : name;
    }
}
