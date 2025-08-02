using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HotPreview.SharedModel;

public static class StringUtilities
{
    /// <summary>
    /// Converts string to start case. Matches the behavior of Lodash's startCase function, which is
    /// used by Storybook for automatically creating story names from exported function names.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The converted string in start case format.</returns>
    public static string StartCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input ?? string.Empty;
        }

        // Split the string into words using regex that handles:
        // - camelCase/PascalCase boundaries
        // - Multiple consecutive separators (dashes, underscores, spaces)
        // - Numbers
        var words = Regex.Split(input, @"[\s\-_]+|(?=[A-Z][a-z])|(?<=[a-z])(?=[A-Z])|(?<=[a-zA-Z])(?=\d)|(?<=\d)(?=[a-zA-Z])")
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .ToArray();

        if (words.Length == 0)
        {
            return string.Empty;
        }

        var result = new StringBuilder();

        for (int i = 0; i < words.Length; i++)
        {
            if (i > 0)
            {
                result.Append(' ');
            }

            var word = words[i].Trim();
            if (word.Length > 0)
            {
                result.Append(char.ToUpper(word[0], CultureInfo.InvariantCulture));
                if (word.Length > 1)
                {
                    result.Append(word.Substring(1).ToLower(CultureInfo.InvariantCulture));
                }
            }
        }

        return result.ToString();
    }
}
