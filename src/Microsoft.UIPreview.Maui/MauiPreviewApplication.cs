using System;

using Microsoft.UIPreview;
using Microsoft.UIPreview.App;
using Microsoft.UIPreview.Maui;
using Microsoft.UIPreview.Maui.Pages;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;


#if ANDROID
using Android.Runtime;
using Android.Views;
#endif

#if !MICROSOFT_PREVIEW_IN_TAP
[assembly: PreviewApplicationClass(typeof(MauiPreviewApplication))]

[assembly: PageUIComponentBaseType(MauiPreviewApplication.MauiPlatformType, "Microsoft.Maui.Controls.Page")]
//[assembly: ControlUIComponentBaseType(MauiPreviewApplication.MauiPlatformType, "Microsoft.Maui.Controls.View")]
#endif

namespace Microsoft.UIPreview.Maui;

public class MauiPreviewApplication : PreviewApplication
{
    private static readonly Lazy<MauiPreviewApplication> s_lazyInstance = new Lazy<MauiPreviewApplication>(() => new MauiPreviewApplication());

    public static MauiPreviewApplication Instance => s_lazyInstance.Value;

    public const string MauiPlatformType = "MAUI";

    public MauiPreviewAppService PreviewAppService { get; set; } = new MauiPreviewAppService();

    public MauiPreviewNavigatorService PreviewNavigatorService { get; set; } = new MauiPreviewNavigatorService();

    private bool _navigatingToPreview = false;
    private int _savedNavigationStackCount = 0;

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
            int currentNavigationStackCount = Application.Current!.MainPage!.Navigation.NavigationStack.Count;
            if (currentNavigationStackCount > _savedNavigationStackCount)
            {
                int amountToPop = currentNavigationStackCount - _savedNavigationStackCount;
                for (int i = 0; i < amountToPop; i++)
                {
                    _ = Application.Current!.MainPage!.Navigation.PopAsync();
                }
            }

            _navigatingToPreview = false;
            _savedNavigationStackCount = 0;
        }

        await Application.Current!.MainPage!.Navigation.PushModalAsync(new PreviewsPage());
    }

    public void NavigateToPageAsync(Microsoft.Maui.Controls.Page page)
    {
        _ = Application.Current!.MainPage!.Navigation.PopModalAsync();
    }

    public void PrepareToNavigateToPreview()
    {
        _navigatingToPreview = true;
        _savedNavigationStackCount = Application.Current!.MainPage!.Navigation.NavigationStack.Count - 1;
        _ = Application.Current!.MainPage!.Navigation.PopModalAsync();
    }
}
