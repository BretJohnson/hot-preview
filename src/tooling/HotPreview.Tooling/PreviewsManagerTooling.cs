using System;
using System.Collections.Generic;
using System.Linq;
using HotPreview.SharedModel;

namespace HotPreview.Tooling;

public class PreviewsManagerTooling(
    IReadOnlyDictionary<string, UIComponentTooling> uiComponents,
    IReadOnlyDictionary<string, UIComponentCategory> categories,
    IReadOnlyDictionary<string, PreviewCommandTooling> commands) : PreviewsManagerBase<UIComponentTooling, PreviewTooling, PreviewCommandTooling>(uiComponents, categories, commands)
{
    private Dictionary<string, List<UIComponentTooling>>? _uiComponentsBySimpleName;

    /// <summary>
    /// Populates the _uiComponentsBySimpleName dictionary on first use.
    /// </summary>
    private void PopulateUIComponentsBySimpleName()
    {
        if (_uiComponentsBySimpleName is not null)
        {
            return;
        }

        _uiComponentsBySimpleName = new Dictionary<string, List<UIComponentTooling>>();

        foreach (UIComponentTooling component in UIComponents)
        {
            string simpleName = component.Name.Split('.').Last();

            if (!_uiComponentsBySimpleName.TryGetValue(simpleName, out List<UIComponentTooling>? components))
            {
                components = new List<UIComponentTooling>();
                _uiComponentsBySimpleName[simpleName] = components;
            }

            components.Add(component);
        }
    }

    /// <summary>
    /// Gets the shortest unique name for a UI component.
    /// Returns the simple name if unique, otherwise the shortest name that is unique at dot boundaries.
    /// This is used for snapshot file names, to keep them short but ensure there are no collisions.
    /// </summary>
    /// <param name="uiComponentName">The full name of the UI component.</param>
    /// <returns>The shortest unique name.</returns>
    /// <exception cref="ArgumentException">Thrown when the UI component doesn't exist.</exception>
    public string GetUIComponentShortName(string uiComponentName)
    {
        if (GetUIComponent(uiComponentName) is null)
        {
            throw new ArgumentException($"UI component '{uiComponentName}' not found.", nameof(uiComponentName));
        }

        PopulateUIComponentsBySimpleName();

        string[] nameParts = uiComponentName.Split('.');
        string simpleName = nameParts.Last();

        // If the simple name is unique, return it
        if (_uiComponentsBySimpleName![simpleName].Count == 1)
        {
            return simpleName;
        }

        // Find the shortest name that is unique
        for (int segmentCount = 2; segmentCount <= nameParts.Length; segmentCount++)
        {
            string candidateName = string.Join(".", nameParts.Skip(nameParts.Length - segmentCount));

            // Check if this candidate name is unique among all components with the same simple name
            List<UIComponentTooling> componentsWithSameSimpleName = _uiComponentsBySimpleName[simpleName];
            IEnumerable<UIComponentTooling> conflictingComponents = componentsWithSameSimpleName
                .Where(c => c.Name != uiComponentName)
                .Where(c =>
                {
                    string[] otherParts = c.Name.Split('.');
                    if (otherParts.Length < segmentCount)
                    {
                        return false;
                    }
                    string otherCandidateName = string.Join(".", otherParts.Skip(otherParts.Length - segmentCount));
                    return otherCandidateName == candidateName;
                });

            if (!conflictingComponents.Any())
            {
                return candidateName;
            }
        }

        // Fallback to full name if no shorter unique name found
        return uiComponentName;
    }
}
