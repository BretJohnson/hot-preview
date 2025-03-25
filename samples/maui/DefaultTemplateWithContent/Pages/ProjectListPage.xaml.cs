namespace DefaultTemplateWithContent.Pages;

public partial class ProjectListPage : ContentPage
{
    public ProjectListPage(ProjectListPageModel model)
    {
        BindingContext = model;
        InitializeComponent();
    }

#if PREVIEWS
    [Preview]
    public static ProjectListPage Preview() => new ProjectListPage(new ProjectListPageModel(null));
#endif
}
