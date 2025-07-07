using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace PreviewFramework.DevToolsApp;

/// <summary>
/// Manages single instance behavior for the DevTools application.
/// Prevents multiple instances and focuses existing window when a new instance is attempted.
/// </summary>
internal static class SingleInstanceManager
{
    private static readonly string MutexName = "PreviewFramework.DevToolsApp.SingleInstance";
    private static Mutex? _mutex;

    /// <summary>
    /// Checks if this is the first instance of the application.
    /// If another instance is already running, attempts to activate it and returns false.
    /// </summary>
    /// <returns>True if this is the first instance, false if another instance is already running.</returns>
    public static bool IsFirstInstance()
    {
        try
        {
            // Try to create or open the mutex
            _mutex = new Mutex(true, MutexName, out bool createdNew);

            if (createdNew)
            {
                // This is the first instance
                return true;
            }
            else
            {
                // Another instance is already running, try to activate it
                TryActivateExistingInstance();
                return false;
            }
        }
        catch (Exception ex)
        {
            // If mutex creation fails, assume we're the first instance to avoid blocking startup
            Debug.WriteLine($"SingleInstanceManager: Error checking for existing instance: {ex.Message}");
            return true;
        }
    }

    /// <summary>
    /// Attempts to activate an existing instance of the application.
    /// </summary>
    private static void TryActivateExistingInstance()
    {
        try
        {
            // Try to find and activate the existing window
            if (OperatingSystem.IsWindows())
            {
                ActivateExistingWindowOnWindows();
            }
            else
            {
                // On non-Windows platforms, we'll rely on the OS to handle window activation
                // when the user attempts to launch the app again
                Debug.WriteLine("SingleInstanceManager: Existing instance detected on non-Windows platform");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SingleInstanceManager: Error activating existing instance: {ex.Message}");
        }
    }

    /// <summary>
    /// Attempts to find and activate the existing DevTools window on Windows.
    /// </summary>
    private static void ActivateExistingWindowOnWindows()
    {
        try
        {
            // Find by process name
            Process[] processes = Process.GetProcessesByName("PreviewFramework.DevToolsApp");

            foreach (Process process in processes)
            {
                if (process.Id != Environment.ProcessId && !process.HasExited)
                {
                    IntPtr mainWindowHandle = process.MainWindowHandle;
                    if (mainWindowHandle != IntPtr.Zero)
                    {
                        ActivateWindow(mainWindowHandle);
                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SingleInstanceManager: Error activating window on Windows: {ex.Message}");
        }
    }

    /// <summary>
    /// Activates and brings a window to the foreground.
    /// </summary>
    private static void ActivateWindow(IntPtr windowHandle)
    {
        // Restore window if minimized and bring to foreground
        WindowsNativeMethods.ShowWindow(windowHandle, WindowsNativeMethods.SW_RESTORE);
        WindowsNativeMethods.SetForegroundWindow(windowHandle);
        WindowsNativeMethods.SetWindowPos(windowHandle, WindowsNativeMethods.HWND_TOP,
            0, 0, 0, 0, WindowsNativeMethods.SWP_NOMOVE | WindowsNativeMethods.SWP_NOSIZE | WindowsNativeMethods.SWP_SHOWWINDOW);
    }

    /// <summary>
    /// Releases the mutex when the application is shutting down.
    /// </summary>
    public static void ReleaseMutex()
    {
        try
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            _mutex = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SingleInstanceManager: Error releasing mutex: {ex.Message}");
        }
    }

    /// <summary>
    /// Windows-specific native methods for window management.
    /// </summary>
    private static class WindowsNativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public const int SW_RESTORE = 9;
        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_SHOWWINDOW = 0x0040;
    }
}
