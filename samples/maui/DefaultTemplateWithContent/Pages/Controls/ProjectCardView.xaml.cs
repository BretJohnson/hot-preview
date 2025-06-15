namespace DefaultTemplateWithContent.Pages.Controls;

public partial class ProjectCardView
{
    public ProjectCardView()
    {
        InitializeComponent();
    }

#if PREVIEWS
    [Preview]
    public static ProjectCardView Preview() => CreateViewWithBinding<ProjectCardView>(MockData.Activate().Project);
#endif
}
