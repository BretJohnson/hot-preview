using System.Net;
using HotPreview.Tooling.McpServer;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HotPreview.Tooling.Tests.McpServer;

[TestClass]
public class SimpleEndpointTest
{
    [TestMethod]
    public async Task TestDirectMcpCall()
    {
        // Arrange
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger<McpHttpServerService> logger = loggerFactory.CreateLogger<McpHttpServerService>();
        McpHttpServerService service = new McpHttpServerService(logger);
        CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using HttpClient httpClient = new HttpClient();
            string baseUrl = service.ServerUrl;

            // Test root endpoint
            HttpResponseMessage rootResponse = await httpClient.GetAsync($"{baseUrl}/", cancellationToken);
            Console.WriteLine($"GET /: {rootResponse.StatusCode}");

            // Test /mcp with various HTTP methods
            HttpResponseMessage getResponse = await httpClient.GetAsync($"{baseUrl}/mcp", cancellationToken);
            Console.WriteLine($"GET /mcp: {getResponse.StatusCode}");

            // Try the correct JSON-RPC POST request
            string jsonRequest = """{"jsonrpc":"2.0","id":"test","method":"tools/list","params":{}}""";
            StringContent content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage postResponse = await httpClient.PostAsync($"{baseUrl}/mcp", content, cancellationToken);
            Console.WriteLine($"POST /mcp with JSON-RPC: {postResponse.StatusCode}");

            if (postResponse.StatusCode != HttpStatusCode.NotFound)
            {
                string responseText = await postResponse.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Response: {responseText}");
            }

            // This test always passes - we're just investigating
            Assert.IsTrue(true);
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }
}
