namespace DefaultTemplateWithContent.Pages.Controls;

public partial class ProjectCardView
{
    public ProjectCardView()
    {
        InitializeComponent();
    }

#if EXAMPLES
    [Example]
    public static ProjectCardView Example() => CreateViewWithBinding<ProjectCardView>(MockData.Activate().Project);
#endif
}
