using System.ComponentModel;
using Microsoft.PreviewFramework.App;

namespace Microsoft.PreviewFramework.Maui.ViewModels;

public class PreviewsViewModel : INotifyPropertyChanged
{
    public static PreviewsViewModel Instance => _lazyInstance.Value;

    public static readonly UIComponentCategory UncategorizedCategory = new UIComponentCategory("Uncategorized");

    private static readonly Lazy<PreviewsViewModel> _lazyInstance = new Lazy<PreviewsViewModel>(() => new PreviewsViewModel());

    public IUIPreviewNavigatorService PreviewNavigatorService { get; }

    private List<UIComponentCategory> categories;
    private List<UIComponentCategoryViewModel> uiComponentCategoryViewModels;

    public List<UIComponentCategoryViewModel> PreviewsData => this.uiComponentCategoryViewModels;

    private PreviewsViewModel()
    {
        this.PreviewNavigatorService = new MauiPreviewNavigatorService();

        this.categories = new List<UIComponentCategory>();
        Dictionary<UIComponentCategory, List<UIComponentReflection>> uiComponentsByCategory = [];

        UIComponentsReflection uiComponents = UIPreviewsManagerReflection.Instance.UIComponents;

        // Create a list of UIComponents for each category, including an "Uncategorized" category.
        // Also save off the list of categories that are used, for sorting.
        foreach (UIComponentReflection uiComponent in uiComponents.Components)
        {
            UIComponentCategory? category = uiComponent.Category;

            if (category == null)
            {
                category = UncategorizedCategory;
            }

            if (!uiComponentsByCategory.TryGetValue(category, out List<UIComponentReflection>? uiComponentsForCategory))
            {
                this.categories.Add(category);
                uiComponentsForCategory = [];
                uiComponentsByCategory.Add(category, uiComponentsForCategory);
            }

            uiComponentsForCategory.Add(uiComponent);
        }

        // Sort the categories and components
        this.categories.Sort((category1, category2) => string.Compare(category1.Name, category2.Name, StringComparison.CurrentCultureIgnoreCase));
        foreach (List<UIComponentReflection> componentsForCategory in uiComponentsByCategory.Values)
        {
            componentsForCategory.Sort((component1, component2) => string.Compare(component1.DisplayName, component2.DisplayName, StringComparison.CurrentCultureIgnoreCase));
        }

        this.uiComponentCategoryViewModels = new List<UIComponentCategoryViewModel>();
        foreach (UIComponentCategory category in this.categories)
        {
            this.uiComponentCategoryViewModels.Add(new UIComponentCategoryViewModel(category, uiComponentsByCategory[category]));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged = null;
}
