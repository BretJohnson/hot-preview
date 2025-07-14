using System.Net;
using HotPreview.Tooling.McpServer;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HotPreview.Tooling.Tests.McpServer;

[TestClass]
public class McpEndpointDiscoveryTest
{
    private ILogger<McpHttpServerService> _logger = null!;

    [TestInitialize]
    public void Setup()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<McpHttpServerService>();
    }

    [TestMethod]
    public async Task DiscoverAvailableEndpoints()
    {
        // Arrange
        var service = new McpHttpServerService(_logger);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using var httpClient = new HttpClient();
            var baseUrl = service.ServerUrl;

            // Test various possible endpoints
            var endpointsToTest = new[]
            {
                "/",
                "/mcp",
                "/sse",
                "/health",
                "/api/mcp",
                "/v1/mcp"
            };

            Console.WriteLine($"Testing endpoints on {baseUrl}");

            foreach (var endpoint in endpointsToTest)
            {
                try
                {
                    // Test GET first
                    var getResponse = await httpClient.GetAsync($"{baseUrl}{endpoint}", cancellationToken);
                    Console.WriteLine($"GET {endpoint}: {getResponse.StatusCode}");

                    // Test POST for potential MCP endpoints
                    if (endpoint.Contains("mcp") || endpoint == "/")
                    {
                        var jsonContent = new StringContent(
                            """{"jsonrpc":"2.0","id":"test","method":"tools/list","params":{}}""",
                            System.Text.Encoding.UTF8,
                            "application/json");

                        var postResponse = await httpClient.PostAsync($"{baseUrl}{endpoint}", jsonContent, cancellationToken);
                        Console.WriteLine($"POST {endpoint}: {postResponse.StatusCode}");

                        if (postResponse.StatusCode != HttpStatusCode.NotFound)
                        {
                            var content = await postResponse.Content.ReadAsStringAsync(cancellationToken);
                            Console.WriteLine($"  Response: {content}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{endpoint}: Exception - {ex.Message}");
                }
            }

            // This test always passes - it's for discovery
            Assert.IsTrue(true);
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task TestMcpEndpointWithDifferentContentTypes()
    {
        // Arrange
        var service = new McpHttpServerService(_logger);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using var httpClient = new HttpClient();
            var baseUrl = service.ServerUrl;

            var request = """{"jsonrpc":"2.0","id":"test","method":"tools/list","params":{}}""";

            // Test different content types
            var contentTypes = new[]
            {
                "application/json",
                "application/json; charset=utf-8",
                "application/jsonrpc",
                "text/plain"
            };

            Console.WriteLine($"Testing /mcp endpoint with different content types on {baseUrl}");

            foreach (var contentType in contentTypes)
            {
                try
                {
                    var content = new StringContent(request, System.Text.Encoding.UTF8, contentType);
                    var response = await httpClient.PostAsync($"{baseUrl}/mcp", content, cancellationToken);
                    Console.WriteLine($"Content-Type {contentType}: {response.StatusCode}");

                    if (response.StatusCode != HttpStatusCode.NotFound)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        Console.WriteLine($"  Response: {responseContent.Substring(0, Math.Min(100, responseContent.Length))}...");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Content-Type {contentType}: Exception - {ex.Message}");
                }
            }

            // This test always passes - it's for discovery
            Assert.IsTrue(true);
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }
}
