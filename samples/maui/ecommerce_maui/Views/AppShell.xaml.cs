namespace EcommerceMAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

#if PREVIEWS
        this.EnablePreviewUI();
#endif
    }
}
