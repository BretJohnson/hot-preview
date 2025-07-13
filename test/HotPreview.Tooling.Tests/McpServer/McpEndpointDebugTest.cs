using Microsoft.VisualStudio.TestTools.UnitTesting;
using HotPreview.Tooling.McpServer;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace HotPreview.Tooling.Tests.McpServer;

[TestClass]
public class McpEndpointDebugTest
{
    [TestMethod]
    public async Task DebugMcpEndpointIssue()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var logger = loggerFactory.CreateLogger<McpHttpServerService>();
        var service = new McpHttpServerService(logger);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using var httpClient = new HttpClient();
            var baseUrl = service.ServerUrl;

            // Test the /mcp endpoint directly
            var jsonRequest = """{"jsonrpc":"2.0","id":"test","method":"tools/list","params":{}}""";
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync($"{baseUrl}/mcp", content, cancellationToken);
            
            Console.WriteLine($"POST /mcp Status: {response.StatusCode}");
            Console.WriteLine($"POST /mcp Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");
            
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"Response Body: {responseText}");
            
            // If 404, let's check the /sse endpoint 
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                var sseResponse = await httpClient.PostAsync($"{baseUrl}/sse", content, cancellationToken);
                Console.WriteLine($"POST /sse Status: {sseResponse.StatusCode}");
                
                if (sseResponse.StatusCode != HttpStatusCode.NotFound)
                {
                    var sseResponseText = await sseResponse.Content.ReadAsStringAsync(cancellationToken);
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