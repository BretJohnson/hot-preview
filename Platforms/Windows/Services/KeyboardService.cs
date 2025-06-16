using DefaultTemplateWithContent.Services;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Windows.System;
using WinRT.Interop;

namespace DefaultTemplateWithContent.WinUI.Services;

public class KeyboardService : IKeyboardService
{
    public event EventHandler<KeyboardEventArgs>? KeyEvent;

    private Window? _window;
    private InputKeyboardSource? _keyboardSource;

    public void Initialize()
    {
        // Get the Window
        _window = Microsoft.Maui.Platform.WindowExtensions.GetActiveWindow(
            Microsoft.Maui.Controls.Application.Current!.Windows[0].Handler!.PlatformView);

        // Get the window handle
        var windowHandle = WindowNative.GetWindowHandle(_window);

        // Get keyboard input source
        var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
        _keyboardSource = InputKeyboardSource.GetForWindowId(windowId);

        if (_keyboardSource != null)
        {
            _keyboardSource.KeyDown += OnKeyDown;
            _keyboardSource.KeyUp += OnKeyUp;
        }
    }

    private void OnKeyDown(InputKeyboardSource sender, KeyEventArgs args)
    {
        var keyArgs = new KeyboardEventArgs((uint)args.VirtualKey, true);
        KeyEvent?.Invoke(this, keyArgs);
        args.Handled = keyArgs.Handled;
    }

    private void OnKeyUp(InputKeyboardSource sender, KeyEventArgs args)
    {
        var keyArgs = new KeyboardEventArgs((uint)args.VirtualKey, false);
        KeyEvent?.Invoke(this, keyArgs);
        args.Handled = keyArgs.Handled;
    }
}
