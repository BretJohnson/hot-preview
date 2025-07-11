using HotPreview.DevToolsApp;
using Uno.UI.Hosting;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Check if this is the first instance of the application
        if (!SingleInstanceManager.IsFirstInstance())
        {
            // Another instance is already running and has been activated
            return;
        }

        try
        {
            var host = UnoPlatformHostBuilder.Create()
                .App(() => new App())
                .UseX11()
                .UseLinuxFrameBuffer()
                .UseMacOS()
                .UseWin32()
                .Build();

            host.Run();
        }
        finally
        {
            // Release the mutex when the application shuts down
            SingleInstanceManager.ReleaseMutex();
        }
    }
}
