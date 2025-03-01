using Microsoft.UIPreview.Maui.Pages;

namespace Microsoft.UIPreview.Maui;

public class MauiPreviewsApplication
{
    public static MauiPreviewsApplication Instance
    {
        get
        {
            if (MauiPreviewsApplication.instance == null)
                throw new InvalidOperationException("MauiPreviewsApplication.Init hasn't been called");
            else return instance;
        }
    }

    private static MauiPreviewsApplication? instance;

    private bool navigatingToPreview = false;
    private int savedNavigationStackCount = 0;

    public Application Application { get; }

    internal static void Init(Application application)
    {
        if (instance != null)
            throw new InvalidOperationException("MauiPreviewsApplication.Init has already been called");

        instance = new MauiPreviewsApplication(application);
    }

    private MauiPreviewsApplication(Application application)
    {
        this.Application = application;
    }

    public async Task ShowPreviewsAsync()
    {
        if (this.navigatingToPreview)
        {
            // The user may navigate around while inside a preview. If they do that, pop the navigation
            // stack back to where it was before they navigated to the preview.
            int currentNavigationStackCount = this.Application.MainPage!.Navigation.NavigationStack.Count;
            if (currentNavigationStackCount > this.savedNavigationStackCount)
            {
                int amountToPop = currentNavigationStackCount - this.savedNavigationStackCount;
                for (int i = 0; i < amountToPop; i++)
                {
                    _ = this.Application.MainPage!.Navigation.PopAsync();
                }
            }

            this.navigatingToPreview = false;
            this.savedNavigationStackCount = 0;
        }

        await this.Application.MainPage!.Navigation.PushModalAsync(new PreviewsPage());
    }

    public void NavigateToPageAsync(Page page)
    {
        _ = this.Application.MainPage!.Navigation.PopModalAsync();
    }

    public void PrepareToNavigateToPreview()
    {
        this.navigatingToPreview = true;
        this.savedNavigationStackCount = this.Application.MainPage!.Navigation.NavigationStack.Count - 1;
        _ = this.Application.MainPage!.Navigation.PopModalAsync();
    }
}
