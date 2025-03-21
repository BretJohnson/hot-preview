using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.UIPreview.App;

namespace Microsoft.UIPreview.Maui.ViewModels;

public class PreviewsViewModel // : INotifyPropertyChanged
{
    public static readonly UIComponentCategory UncategorizedCategory = new("Uncategorized");
    private static readonly Lazy<PreviewsViewModel> s_lazyInstance = new Lazy<PreviewsViewModel>(() => new PreviewsViewModel());

    public static PreviewsViewModel Instance => s_lazyInstance.Value;

    public IReadOnlyList<PreviewsItemViewModel> PreviewsItems { get; }
    public bool HasCategories { get; }
    //public event PropertyChangedEventHandler? PropertyChanged = null;

    private PreviewsViewModel()
    {
        var categories = new List<UIComponentCategory>();
        Dictionary<UIComponentCategory, List<UIComponentReflection>> uiComponentsByCategory = [];

        UIComponentsReflection uiComponents = PreviewsManagerReflection.Instance.UIComponents;

        // Create a list of UIComponents for each category, including an "Uncategorized" category.
        foreach (UIComponentReflection uiComponent in uiComponents.Components)
        {
            UIComponentCategory? category = uiComponent.Category;

            category ??= UncategorizedCategory;

            if (!uiComponentsByCategory.TryGetValue(category, out List<UIComponentReflection>? uiComponentsForCategory))
            {
                categories.Add(category);
                uiComponentsForCategory = [];
                uiComponentsByCategory.Add(category, uiComponentsForCategory);
            }

            uiComponentsForCategory.Add(uiComponent);
        }

        // Sort the categories and components
        categories.Sort((category1, category2) => string.Compare(category1.Name, category2.Name, StringComparison.CurrentCultureIgnoreCase));
        foreach (List<UIComponentReflection> componentsForCategory in uiComponentsByCategory.Values)
        {
            componentsForCategory.Sort((component1, component2) => string.Compare(component1.DisplayName, component2.DisplayName, StringComparison.CurrentCultureIgnoreCase));
        }

        var previewsItems = new List<PreviewsItemViewModel>();

        HasCategories = categories.Count > 1;

        foreach (UIComponentCategory category in categories)
        {
            if (HasCategories)
            {
                previewsItems.Add(new UIComponentCategoryViewModel(category));
            }

            foreach (UIComponentReflection uiComponent in uiComponentsByCategory[category])
            {
                previewsItems.Add(new UIComponentViewModel(uiComponent));

                if (uiComponent.HasMultiplePreviews)
                {
                    foreach (PreviewReflection preview in uiComponent.Previews)
                    {
                        previewsItems.Add(new PreviewViewModel(preview));
                    }
                }
            }
        }

        PreviewsItems = previewsItems;
    }

    public void NavigateToPreview(PreviewReflection preview)
    {
        MauiPreviewApplication.Instance.PreviewNavigatorService.NavigateToPreview(preview);
    }
}
