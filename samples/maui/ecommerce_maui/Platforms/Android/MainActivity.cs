using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;

namespace EcommerceMAUI;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
#if PREVIEWS
    public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
    {
        return MauiPreviewApplication.Instance.OnKeyDownForPreviewUI(keyCode, e) || base.OnKeyDown(keyCode, e);
    }
#endif
}
