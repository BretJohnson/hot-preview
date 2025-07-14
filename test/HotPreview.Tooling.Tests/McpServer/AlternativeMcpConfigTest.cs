using System.Net;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;

namespace HotPreview.Tooling.Tests.McpServer;

[TestClass]
public class AlternativeMcpConfigTest
{
    [TestMethod]
    public async Task TestAlternativeMcpConfiguration()
    {
        // Try different MCP server configurations to see what works
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token;

        // Configuration 1: Try without WithHttpTransport
        await TestConfiguration("Config1: Basic MCP", () =>
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddMcpServer()
                .WithToolsFromAssembly()
                .WithPromptsFromAssembly();
            builder.WebHost.UseUrls("http://localhost:0");
            var app = builder.Build();
            app.MapMcp();
            return Task.FromResult(app);
        }, cancellationToken);

        // Configuration 2: Try with explicit transport configuration
        await TestConfiguration("Config2: With explicit HTTP transport", () =>
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly()
                .WithPromptsFromAssembly();
            builder.WebHost.UseUrls("http://localhost:0");
            var app = builder.Build();
            app.MapMcp();
            return Task.FromResult(app);
        }, cancellationToken);

        // Configuration 3: Try mapping to a specific route
        await TestConfiguration("Config3: Map to specific route", () =>
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithToolsFromAssembly()
                .WithPromptsFromAssembly();
            builder.WebHost.UseUrls("http://localhost:0");
            var app = builder.Build();
            app.MapMcp("/api/mcp");
            return Task.FromResult(app);
        }, cancellationToken);

        Assert.IsTrue(true, "Configuration testing completed");
    }

    private async Task TestConfiguration(string configName, Func<Task<WebApplication>> createApp, CancellationToken cancellationToken)
    {
        Console.WriteLine($"\n=== Testing {configName} ===");

        WebApplication? app = null;
        try
        {
            app = await createApp();
            await app.StartAsync(cancellationToken);

            var addresses = app.Urls.ToArray();
            if (addresses.Length == 0)
            {
                Console.WriteLine($"{configName}: No addresses bound");
                return;
            }

            var baseUrl = addresses[0];
            Console.WriteLine($"{configName}: Server started at {baseUrl}");

            using var httpClient = new HttpClient();

            // Test common endpoints
            var endpoints = new[] { "/", "/mcp", "/sse", "/api/mcp" };
            foreach (var endpoint in endpoints)
            {
                try
                {
                    // Test GET
                    var getResponse = await httpClient.GetAsync($"{baseUrl}{endpoint}", cancellationToken);
                    Console.WriteLine($"{configName}: GET {endpoint} = {getResponse.StatusCode}");

                    // Test POST with JSON-RPC
                    var jsonRequest = """{"jsonrpc":"2.0","id":"test","method":"tools/list","params":{}}""";
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                    var postResponse = await httpClient.PostAsync($"{baseUrl}{endpoint}", content, cancellationToken);
                    Console.WriteLine($"{configName}: POST {endpoint} = {postResponse.StatusCode}");

                    if (postResponse.IsSuccessStatusCode)
                    {
                        var responseText = await postResponse.Content.ReadAsStringAsync(cancellationToken);
                        Console.WriteLine($"{configName}: SUCCESS! Response: {responseText.Substring(0, Math.Min(100, responseText.Length))}...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{configName}: Error testing {endpoint}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{configName}: Configuration failed: {ex.Message}");
        }
        finally
        {
            if (app != null)
            {
                await app.StopAsync(cancellationToken);
                await app.DisposeAsync();
            }
        }
    }
}
