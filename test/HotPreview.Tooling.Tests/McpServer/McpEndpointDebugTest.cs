using System.Net;
using System.Text;
using HotPreview.Tooling.McpServer;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HotPreview.Tooling.Tests.McpServer;

[TestClass]
public class McpEndpointDebugTest
{
    [TestMethod]
    public async Task DebugMcpEndpointIssue()
    {
        // Arrange
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        ILogger<McpHttpServerService> logger = loggerFactory.CreateLogger<McpHttpServerService>();
        McpHttpServerService service = new McpHttpServerService(logger);
        CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using HttpClient httpClient = new HttpClient();
            string baseUrl = service.ServerUrl;

            // Test the /mcp endpoint directly
            string jsonRequest = """{"jsonrpc":"2.0","id":"test","method":"tools/list","params":{}}""";
            StringContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync($"{baseUrl}/mcp", content, cancellationToken);

            Console.WriteLine($"POST /mcp Status: {response.StatusCode}");
            Console.WriteLine($"POST /mcp Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");

            string responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"Response Body: {responseText}");

            // If 404, let's check the /sse endpoint 
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                HttpResponseMessage sseResponse = await httpClient.PostAsync($"{baseUrl}/sse", content, cancellationToken);
                Console.WriteLine($"POST /sse Status: {sseResponse.StatusCode}");

                if (sseResponse.StatusCode != HttpStatusCode.NotFound)
                {
                    string sseResponseText = await sseResponse.Content.ReadAsStringAsync(cancellationToken);
                    Console.WriteLine($"SSE Response: {sseResponseText}");
                }
            }

            // This test is just for debugging
            Assert.IsTrue(true, "Debug test completed");
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }
}
