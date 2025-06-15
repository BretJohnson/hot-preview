namespace DefaultTemplateWithContent.Pages.Controls;

public partial class AddButton
{
    public AddButton()
    {
        InitializeComponent();
    }

#if PREVIEWS
    [Preview]
    public static AddButton Preview() => new();
#endif
}
