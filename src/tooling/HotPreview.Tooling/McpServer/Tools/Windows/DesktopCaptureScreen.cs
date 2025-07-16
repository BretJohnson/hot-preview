using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ModelContextProtocol.Server;

namespace HotPreview.Tooling.McpServer.Tools.Windows;

[McpServerToolType]
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public class DesktopCaptureScreen
{
    /// <summary>
    /// Takes a screenshot of the entire primary screen and saves it to the specified path.
    /// </summary>
    /// <param name="outputPath">The full path where the screenshot will be saved.</param>
    /// <returns>A status message indicating success or failure.</returns>
    [McpServerTool(Name = "take_screenshot")]
    [Description("Takes a screenshot of the entire primary screen and saves it to the specified path on user system.")]
    public string TakeScreenshot(string outputPath)
    {
        try
        {
            // Get screen dimensions using Windows API
            int screenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
            int screenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);

            if (screenWidth <= 0 || screenHeight <= 0)
            {
                return "Error: Could not determine screen dimensions.";
            }

            return CaptureScreenRegion(outputPath, 0, 0, screenWidth, screenHeight);
        }
        catch (Exception ex)
        {
            return $"Error taking screenshot: {ex.Message}";
        }
    }

    /// <summary>
    /// Takes a screenshot of a specific region of the screen and saves it to the specified path.
    /// </summary>
    /// <param name="outputPath">The full path where the screenshot will be saved.</param>
    /// <param name="x">The x-coordinate of the top-left corner of the region.</param>
    /// <param name="y">The y-coordinate of the top-left corner of the region.</param>
    /// <param name="width">The width of the region.</param>
    /// <param name="height">The height of the region.</param>
    /// <returns>A status message indicating success or failure.</returns>
    [McpServerTool(Name = "take_region_screenshot")]
    [Description("Takes a screenshot of a specific region of the screen on user system.")]
    public string TakeRegionScreenshot(string outputPath, int x, int y, int width, int height)
    {
        try
        {
            return CaptureScreenRegion(outputPath, x, y, width, height);
        }
        catch (Exception ex)
        {
            return $"Error taking region screenshot: {ex.Message}";
        }
    }

    /// <summary>
    /// Captures a region of the screen using Windows GDI+ APIs.
    /// </summary>
    /// <param name="outputPath">Path to save the captured image.</param>
    /// <param name="x">X-coordinate of the region to capture.</param>
    /// <param name="y">Y-coordinate of the region to capture.</param>
    /// <param name="width">Width of the region to capture.</param>
    /// <param name="height">Height of the region to capture.</param>
    /// <returns>Status message with the result of the operation.</returns>
    private string CaptureScreenRegion(string outputPath, int x, int y, int width, int height)
    {
        IntPtr desktopPtr = NativeMethods.GetDesktopWindow();
        IntPtr desktopDc = NativeMethods.GetDC(desktopPtr);
        IntPtr memoryDc = NativeMethods.CreateCompatibleDC(desktopDc);
        IntPtr bitmapPtr = NativeMethods.CreateCompatibleBitmap(desktopDc, width, height);
        IntPtr oldBitmapPtr = NativeMethods.SelectObject(memoryDc, bitmapPtr);

        try
        {
            // Copy the screen content to our bitmap
            bool result = NativeMethods.BitBlt(memoryDc, 0, 0, width, height, desktopDc, x, y, NativeMethods.SRCCOPY);
            if (!result)
            {
                return "Error: Failed to capture screen content.";
            }

            // Select the old bitmap back into the DC
            NativeMethods.SelectObject(memoryDc, oldBitmapPtr);

            // Convert GDI+ bitmap to .NET Bitmap
            using (Bitmap bitmap = Image.FromHbitmap(bitmapPtr))
            {
                // Determine the image format based on file extension
                string extension = Path.GetExtension(outputPath)?.ToLower() ?? ".png";
                ImageFormat format = ImageFormat.Png;

                switch (extension)
                {
                    case ".jpg":
                    case ".jpeg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                    case ".gif":
                        format = ImageFormat.Gif;
                        break;
                }

                // Save the bitmap to file
                bitmap.Save(outputPath, format);
            }

            return $"Screenshot successfully saved to {outputPath}";
        }
        finally
        {
            // Clean up GDI resources
            NativeMethods.DeleteObject(bitmapPtr);
            NativeMethods.DeleteDC(memoryDc);
            NativeMethods.ReleaseDC(desktopPtr, desktopDc);
        }
    }

    /// <summary>
    /// Takes a screenshot of a specific application window and saves it to the specified path.
    /// </summary>
    /// <param name="outputPath">The full path where the screenshot will be saved.</param>
    /// <param name="windowTitle">The title or part of the title of the application window.</param>
    /// <param name="exactMatch">Whether to require an exact match of the window title.</param>
    /// <returns>A status message indicating success or failure.</returns>
    [McpServerTool(Name = "take_application_screenshot")]
    [Description("Takes a screenshot of a specific application window on user system.")]
    public string TakeApplicationScreenshot(string outputPath, string windowTitle, bool exactMatch = false)
    {
        try
        {
            IntPtr windowHandle = IntPtr.Zero;

            // First try to find the window by exact title if exactMatch is true
            if (exactMatch)
            {
                windowHandle = NativeMethods.FindWindow(null, windowTitle);
                if (windowHandle == IntPtr.Zero)
                {
                    return $"Error: Could not find window with exact title \"{windowTitle}\"";
                }
            }
            else
            {
                // Use EnumWindows to find windows with partial title match
                List<WindowInfo> windows = new List<WindowInfo>();
                NativeMethods.EnumWindows((hWnd, lParam) =>
                {
                    if (NativeMethods.IsWindowVisible(hWnd))
                    {
                        int length = NativeMethods.GetWindowTextLength(hWnd);
                        if (length > 0)
                        {
                            var sb = new StringBuilder(length + 1);
                            NativeMethods.GetWindowText(hWnd, sb, sb.Capacity);
                            string title = sb.ToString();

                            if (title.Contains(windowTitle, StringComparison.OrdinalIgnoreCase))
                            {
                                windows.Add(new WindowInfo { Handle = hWnd, Title = title });
                            }
                        }
                    }
                    return true;
                }, IntPtr.Zero);

                if (windows.Count == 0)
                {
                    return $"Error: Could not find any window with title containing \"{windowTitle}\"";
                }

                // For partial matches, use the first window found
                windowHandle = windows[0].Handle;

                // If more than one window was found, include this in the result message
                if (windows.Count > 1)
                {
                    return CaptureWindowToFile(windowHandle, outputPath) +
                           $"\nNote: Found {windows.Count} matching windows. Used \"{windows[0].Title}\". For specific window, use exact title.";
                }
            }

            // Bring the window to the foreground to ensure it's visible
            NativeMethods.SetForegroundWindow(windowHandle);

            // Allow time for the window to come to the foreground
            System.Threading.Thread.Sleep(300);

            return CaptureWindowToFile(windowHandle, outputPath);
        }
        catch (Exception ex)
        {
            return $"Error taking application screenshot: {ex.Message}";
        }
    }

    /// <summary>
    /// Captures a window to a file.
    /// </summary>
    /// <param name="windowHandle">Handle to the window to capture.</param>
    /// <param name="outputPath">Path to save the captured image.</param>
    /// <returns>Status message with the result of the operation.</returns>
    private string CaptureWindowToFile(IntPtr windowHandle, string outputPath)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return "Error: Invalid window handle.";
        }

        // Get window dimensions
        NativeMethods.RECT rect;
        if (!NativeMethods.GetWindowRect(windowHandle, out rect))
        {
            return "Error: Could not get window dimensions.";
        }

        int width = rect.Width;
        int height = rect.Height;

        if (width <= 0 || height <= 0)
        {
            return "Error: Invalid window dimensions.";
        }

        IntPtr windowDc = NativeMethods.GetWindowDC(windowHandle);
        IntPtr memoryDc = NativeMethods.CreateCompatibleDC(windowDc);
        IntPtr bitmapPtr = NativeMethods.CreateCompatibleBitmap(windowDc, width, height);
        IntPtr oldBitmapPtr = NativeMethods.SelectObject(memoryDc, bitmapPtr);

        try
        {
            // Copy the window content to our bitmap
            bool result = NativeMethods.BitBlt(
                memoryDc, 0, 0, width, height,
                windowDc, 0, 0, NativeMethods.SRCCOPY);

            if (!result)
            {
                return "Error: Failed to capture window content.";
            }

            // Select the old bitmap back into the DC
            NativeMethods.SelectObject(memoryDc, oldBitmapPtr);

            // Convert GDI+ bitmap to .NET Bitmap
            using (Bitmap bitmap = Image.FromHbitmap(bitmapPtr))
            {
                // Determine the image format based on file extension
                string extension = Path.GetExtension(outputPath)?.ToLower() ?? ".png";
                ImageFormat format = ImageFormat.Png;

                switch (extension)
                {
                    case ".jpg":
                    case ".jpeg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                    case ".gif":
                        format = ImageFormat.Gif;
                        break;
                }

                // Save the bitmap to file
                bitmap.Save(outputPath, format);
            }

            return $"Application window screenshot successfully saved to {outputPath}";
        }
        finally
        {
            // Clean up GDI resources
            NativeMethods.DeleteObject(bitmapPtr);
            NativeMethods.DeleteDC(memoryDc);
            NativeMethods.ReleaseDC(windowHandle, windowDc);
        }
    }

    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public required string Title { get; set; }
        public string? ClassName { get; set; }
        public uint ProcessId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsVisible { get; set; }
        public bool IsMinimized { get; set; }
        public bool IsCurrentProcess { get; set; } // New field to flag current process
    }

    /// <summary>
    /// Lists all visible application windows with their titles.
    /// </summary>
    /// <returns>A formatted string containing the list of visible windows.</returns>
    [McpServerTool(Name = "list_windows")]
    [Description("Lists all application windows with their titles on user system, highlighting current process.")]
    public string ListWindows()
    {
        try
        {
            var windows = new List<WindowInfo>();
            uint currentProcessId = (uint)System.Diagnostics.Process.GetCurrentProcess().Id; // Get this app's PID

            NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                bool isVisible = NativeMethods.IsWindowVisible(hWnd);
                bool isMinimized = NativeMethods.IsIconic(hWnd);

                int length = NativeMethods.GetWindowTextLength(hWnd);
                StringBuilder sb = new StringBuilder(length + 1);
                NativeMethods.GetWindowText(hWnd, sb, sb.Capacity);
                string title = string.IsNullOrEmpty(sb.ToString()) ? "(No Title)" : sb.ToString();

                StringBuilder className = new StringBuilder(256);
                NativeMethods.GetClassName(hWnd, className, className.Capacity);

                NativeMethods.GetWindowThreadProcessId(hWnd, out uint processId);

                NativeMethods.RECT rect;
                NativeMethods.GetWindowRect(hWnd, out rect);

                windows.Add(new WindowInfo
                {
                    Handle = hWnd,
                    Title = title,
                    ClassName = className.ToString(),
                    ProcessId = processId,
                    Width = rect.Width,
                    Height = rect.Height,
                    IsVisible = isVisible,
                    IsMinimized = isMinimized,
                    IsCurrentProcess = (processId == currentProcessId)
                });

                return true;
            }, IntPtr.Zero);

            windows.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));

            StringBuilder result = new StringBuilder("# All Windows\n\n");
            result.AppendLine("| Window Title | Class Name | PID | Size | Visible | Minimized | Current Process |");
            result.AppendLine("|-------------|------------|-----|------|---------|-----------|-----------------|");

            foreach (WindowInfo window in windows)
            {
                result.AppendLine($"| {window.Title} | {window.ClassName} | {window.ProcessId} | {window.Width}x{window.Height} | {window.IsVisible} | {window.IsMinimized} | {(window.IsCurrentProcess ? "Yes" : "No")} |");
            }

            result.AppendLine($"\nTotal windows: {windows.Count}");
            result.AppendLine($"Current Process ID: {currentProcessId}");

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"Error listing windows: {ex.Message}";
        }
    }

    /// <summary>
    /// Native Windows API functions for getting display information, capturing screenshots, and window management.
    /// </summary>
    private static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight,
            IntPtr hObjSource, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        public const int SRCCOPY = 0x00CC0020;

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;

        // Additional useful methods
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd); // Check if window is minimized

        [DllImport("user32.dll")]
        public static extern bool IsZoomed(IntPtr hWnd); // Check if window is maximized

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const int SW_RESTORE = 9; // Restore minimized window
        public const int SW_MAXIMIZE = 3; // Maximize window
        public const int SW_MINIMIZE = 6; // Minimize window

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
    }
}
