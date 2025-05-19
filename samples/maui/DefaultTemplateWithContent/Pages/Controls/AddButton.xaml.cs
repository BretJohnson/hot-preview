namespace DefaultTemplateWithContent.Pages.Controls;

public partial class AddButton
{
    public AddButton()
    {
        InitializeComponent();
    }

#if EXAMPLES
    [Example]
    public static AddButton Example() => new();
#endif
}
