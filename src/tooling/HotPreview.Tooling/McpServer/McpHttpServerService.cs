using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HotPreview.Tooling.McpServer;

public class McpHttpServerService : IHostedService
{
    private readonly ILogger<McpHttpServerService> _logger;
    private WebApplication? _app;
    private int _port;

    public string ServerUrl => $"http://localhost:{_port}";

    public McpHttpServerService(ILogger<McpHttpServerService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_app is not null)
        {
            throw new InvalidOperationException("MCP HTTP server is already running");
        }

        try
        {
            _port = FindAvailablePort(54243);
            _logger.LogInformation("Starting MCP HTTP server on port {Port}", _port);

            var builder = WebApplication.CreateBuilder();

            // Configure MCP server with existing tools and prompts
            builder.Services
                .AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly()
                .WithPromptsFromAssembly();

            // Configure web host
            builder.WebHost.UseUrls($"http://localhost:{_port}");

            _app = builder.Build();

            // Map MCP endpoints using the library
            _app.MapMcp();

            // Add health check endpoint
            _app.MapGet("/health", () => Results.Ok(new { status = "healthy", url = ServerUrl }));

            await _app.StartAsync(cancellationToken);
            _logger.LogInformation("MCP HTTP server started at {Url}", ServerUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MCP HTTP server");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_app != null)
        {
            _logger.LogInformation("Stopping MCP HTTP server");
            await _app.StopAsync(cancellationToken);
            await _app.DisposeAsync();
            _app = null;
        }
    }

    private static int FindAvailablePort(int startPort)
    {
        for (int port = startPort; port < startPort + 100; port++)
        {
            try
            {
                using var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return port;
            }
            catch (SocketException)
            {
                // Port is in use, try next one
            }
        }

        // Fall back to random port
        using var randomListener = new TcpListener(IPAddress.Loopback, 0);
        randomListener.Start();
        int randomPort = ((IPEndPoint)randomListener.LocalEndpoint).Port;
        randomListener.Stop();
        return randomPort;
    }
}
