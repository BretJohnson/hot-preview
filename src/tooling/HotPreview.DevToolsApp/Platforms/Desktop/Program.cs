using HotPreview.DevToolsApp;
using Serilog;
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
            UnoPlatformHost host = UnoPlatformHostBuilder.Create()
                .App(() => new App())
                .UseX11()
                .UseLinuxFrameBuffer()
                .UseMacOS()
                .UseWin32()
                .Build();

            host.Run();
        }
        catch (Exception ex)
        {
            // Always write to console as it's the most reliable
            Console.Error.WriteLine($"Fatal exception occurred: {ex}");

            // Try to log with Serilog as well, but don't rely on it
            try
            {
                Log.Fatal(ex, "Fatal exception occurred: {Message}", ex.Message);
            }
            catch
            {
                // Ignore logging errors - console output above is our fallback
            }

            // Re-throw to maintain original behavior and exit code
            throw;
        }
        finally
        {
            // Ensure logs are flushed and mutex is released when the application shuts down
            try
            {
                Log.CloseAndFlush();
            }
            catch
            {
                // Ignore any errors during log cleanup
            }

            SingleInstanceManager.ReleaseMutex();
        }
    }
}
