namespace Microsoft.UIPreview;

public class NameUtilities
{
    public static string GetUnqualifiedName(string name)
    {
        int index = name.LastIndexOf('.');
        return index >= 0 ? name.Substring(index + 1) : name;
    }
}
