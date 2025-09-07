using System;
using Microsoft.Maui.Devices;

namespace HotPreview.App.Maui;

public partial class MauiPreviewApplication
{
    public override long? GetDesktopAppProcessId()
    {
        // Only provide PID on macOS (Mac Catalyst). iOS should return null.
        if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
        {
            return Environment.ProcessId;
        }

        return null;
    }
}
