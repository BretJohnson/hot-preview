using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

using Microsoft.UIPreview;
using Microsoft.UIPreview.App;
using Microsoft.UIPreview.Maui;
using Microsoft.UIPreview.Maui.Pages;
using System.Threading.Tasks;


#if ANDROID
using Android.Runtime;
using Android.Views;
#endif

#if !MICROSOFT_PREVIEW_IN_TAP
[assembly: PreviewApplication(typeof(MauiPreviewApplication))]
#endif

namespace Microsoft.UIPreview.Maui;

public class MauiPreviewApplication : PreviewApplication
{
    public static MauiPreviewApplication Instance
    {
        get
        {
            if (s_instance == null)
                throw new InvalidOperationException("MauiPreviewsApplication.Init hasn't been called");
            else return s_instance;
        }
    }

    private static MauiPreviewApplication? s_instance;

    public MauiPreviewAppService PreviewAppService { get; set; } = new MauiPreviewAppService();

    public MauiPreviewNavigatorService PreviewNavigatorService { get; set; } = new MauiPreviewNavigatorService();

    private bool _navigatingToPreview = false;
    private int _savedNavigationStackCount = 0;

    internal static void Init(Application application)
    {
        if (s_instance != null)
            throw new InvalidOperationException("MauiPreviewsApplication.Init has already been called");

        s_instance = new MauiPreviewApplication(application);
    }

    private MauiPreviewApplication(Application application)
    {
        Application = application;
    }

    public Application Application { get; }

#if ANDROID
    public bool OnKeyDownForPreviewUI([GeneratedEnum] Keycode keyCode, KeyEvent e)
    {
        bool isShiftPressed = (e.MetaState & MetaKeyStates.ShiftLeftOn) != 0;
        if (keyCode == Keycode.Grave && isShiftPressed)
        {
            _ = MauiPreviewApplication.Instance.ShowPreviewUIAsync();
            return true;
        }

        return false;
    }
#endif

    public Color? PreviewUIBadgeColor { get; set; } = Colors.Orange;

    public override PreviewAppService GetPreviewAppService() => PreviewAppService;

    public async Task ShowPreviewUIAsync()
    {
        if (_navigatingToPreview)
        {
            // The user may navigate around while inside a preview. If they do that, pop the navigation
            // stack back to where it was before they navigated to the preview.
            int currentNavigationStackCount = Application.MainPage!.Navigation.NavigationStack.Count;
            if (currentNavigationStackCount > _savedNavigationStackCount)
            {
                int amountToPop = currentNavigationStackCount - _savedNavigationStackCount;
                for (int i = 0; i < amountToPop; i++)
                {
                    _ = Application.MainPage!.Navigation.PopAsync();
                }
            }

            _navigatingToPreview = false;
            _savedNavigationStackCount = 0;
        }

        await Application.MainPage!.Navigation.PushModalAsync(new PreviewsPage());
    }

    public void NavigateToPageAsync(Page page)
    {
        _ = Application.MainPage!.Navigation.PopModalAsync();
    }

    public void PrepareToNavigateToPreview()
    {
        _navigatingToPreview = true;
        _savedNavigationStackCount = Application.MainPage!.Navigation.NavigationStack.Count - 1;
        _ = Application.MainPage!.Navigation.PopModalAsync();
    }
}
