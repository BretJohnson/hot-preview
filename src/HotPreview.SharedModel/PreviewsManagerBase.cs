using System.Collections.Generic;
using System.Linq;

namespace HotPreview.SharedModel;

public abstract class PreviewsManagerBase<TUIComponent, TPreview, TCommand>(
    IReadOnlyDictionary<string, TUIComponent> uiComponents,
    IReadOnlyDictionary<string, UIComponentCategory> categories,
    IReadOnlyDictionary<string, TCommand> commands)
    where TUIComponent : UIComponentBase<TPreview>
    where TPreview : PreviewBase
    where TCommand : PreviewCommandBase
{
    private readonly IReadOnlyDictionary<string, TUIComponent> _uiComponentsByName = uiComponents;
    private readonly IReadOnlyDictionary<string, UIComponentCategory> _categories = categories;
    private readonly IReadOnlyDictionary<string, TCommand> _commandsByName = commands;
    private IReadOnlyList<(UIComponentCategory Category, IReadOnlyList<TUIComponent> UIComponents)>? _categorizedUIComponents;
    private Dictionary<string, List<TUIComponent>>? _uiComponentsBySimpleName;

    public IEnumerable<UIComponentCategory> Categories => _categories.Values;

    public IEnumerable<TUIComponent> UIComponents => _uiComponentsByName.Values;

    public IEnumerable<TCommand> Commands => _commandsByName.Values;


    /// <summary>
    /// Get UI components grouped by category, sorted by category name, with UI components sorted by display name.
    /// Includes "Pages" and "Controls" categories for uncategorized components based on their Kind.
    /// </summary>
    public IReadOnlyList<(UIComponentCategory Category, IReadOnlyList<TUIComponent> UIComponents)> CategorizedUIComponents
    {
        get
        {
            if (_categorizedUIComponents is null)
            {
                var result = new List<(UIComponentCategory Category, IReadOnlyList<TUIComponent> UIComponents)>();

                // Add existing categories
                IEnumerable<(UIComponentCategory category, IReadOnlyList<TUIComponent> categoryUIComponents)> categorizedResult = _categories.Values
                    .Select(category =>
                    {
                        IReadOnlyList<TUIComponent> categoryUIComponents = category.UIComponentNames
                            .Select(name => GetUIComponent(name))
                            .OfType<TUIComponent>()  // Filters out nulls
                            .OrderBy(component => component.DisplayName)
                            .ToList();
                        return (category, categoryUIComponents);
                    });

                result.AddRange(categorizedResult);

                // Add uncategorized components separated by type
                var categorizedComponentNames = new HashSet<string>(_categories.Values
                    .SelectMany(category => category.UIComponentNames));

                var uncategorizedComponents = _uiComponentsByName.Values
                    .Where(component => !categorizedComponentNames.Contains(component.Name))
                    .ToList();

                // Group uncategorized components by Kind
                var uncategorizedPages = uncategorizedComponents
                    .Where(component => component.Kind == UIComponentKind.Page)
                    .OrderBy(component => component.DisplayName)
                    .ToList();

                var uncategorizedControls = uncategorizedComponents
                    .Where(component => component.Kind == UIComponentKind.Control)
                    .OrderBy(component => component.DisplayName)
                    .ToList();

                // Add Pages category if there are uncategorized pages
                if (uncategorizedPages.Count > 0)
                {
                    var pagesCategory = new UIComponentCategory("Pages", []);
                    result.Add((pagesCategory, uncategorizedPages));
                }

                // Add Controls category if there are uncategorized controls
                if (uncategorizedControls.Count > 0)
                {
                    var controlsCategory = new UIComponentCategory("Controls", []);
                    result.Add((controlsCategory, uncategorizedControls));
                }

                // Sort all categories by name
                _categorizedUIComponents = result
                    .OrderBy(item => item.Category.Name)
                    .ToList();
            }

            return _categorizedUIComponents;
        }
    }

    public bool HasCategories => _categories.Count > 0;

    public TUIComponent? GetUIComponent(string name) =>
        _uiComponentsByName.TryGetValue(name, out TUIComponent? uiComponent) ? uiComponent : null;

    public TCommand? GetCommand(string name) =>
        _commandsByName.TryGetValue(name, out TCommand? command) ? command : null;

    /// <summary>
    /// Returns true if the manager contains the specified UI component and that component contains the specified preview.
    /// </summary>
    /// <param name="uiComponentName">The name of the UI component to check.</param>
    /// <param name="previewName">The name of the preview to check.</param>
    /// <returns>True if both the UI component and preview exist; otherwise, false.</returns>
    public bool HasPreview(string uiComponentName, string previewName)
    {
        TUIComponent? uiComponent = GetUIComponent(uiComponentName);
        return uiComponent?.GetPreview(previewName) is not null;
    }

    /// <summary>
    /// Populates the _uiComponentsBySimpleName dictionary on first use.
    /// </summary>
    private void PopulateUIComponentsBySimpleName()
    {
        if (_uiComponentsBySimpleName is not null)
        {
            return;
        }

        _uiComponentsBySimpleName = new Dictionary<string, List<TUIComponent>>();

        foreach (TUIComponent component in _uiComponentsByName.Values)
        {
            string simpleName = component.Name.Split('.').Last();

            if (!_uiComponentsBySimpleName.TryGetValue(simpleName, out List<TUIComponent>? components))
            {
                components = new List<TUIComponent>();
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
    /// <returns>The shortest unique name, or null if the component doesn't exist.</returns>
    public string? GetUIComponentShortName(string uiComponentName)
    {
        if (string.IsNullOrEmpty(uiComponentName) || !_uiComponentsByName.ContainsKey(uiComponentName))
        {
            return null;
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
            List<TUIComponent> componentsWithSameSimpleName = _uiComponentsBySimpleName[simpleName];
            IEnumerable<TUIComponent> conflictingComponents = componentsWithSameSimpleName
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
