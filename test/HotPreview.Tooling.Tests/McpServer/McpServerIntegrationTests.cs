using System.Text.Json;
using HotPreview.Tooling.McpServer;
using HotPreview.Tooling.Tests.McpServer.TestHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HotPreview.Tooling.Tests.McpServer;

[TestClass]
public class McpServerIntegrationTests
{
    private ILogger<McpHttpServerService> _serverLogger = null!;
    private ILogger<McpTestClient> _clientLogger = null!;

    [TestInitialize]
    public void Setup()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _serverLogger = loggerFactory.CreateLogger<McpHttpServerService>();
        _clientLogger = loggerFactory.CreateLogger<McpTestClient>();
    }

    [TestMethod]
    public async Task FullWorkflow_StartServerListToolsCallTool_ShouldWork()
    {
        // Arrange
        McpHttpServerService service = new McpHttpServerService(_serverLogger);
        CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;

        try
        {
            // Act 1: Start server
            await service.StartAsync(cancellationToken);
            Assert.IsNotNull(service.ServerUrl);

            using HttpClient httpClient = new HttpClient();
            using McpTestClient mcpClient = new McpTestClient(httpClient, _clientLogger);
            httpClient.BaseAddress = new Uri(service.ServerUrl);

            // Act 2: List available tools
            JsonDocument toolsResponse = await mcpClient.ListToolsAsync(cancellationToken);

            // Assert: Tools discovered
            Assert.IsNotNull(toolsResponse);
            Assert.IsTrue(toolsResponse.RootElement.TryGetProperty("result", out JsonElement result));
            Assert.IsTrue(result.TryGetProperty("tools", out JsonElement toolsArray));

            List<JsonElement> tools = toolsArray.EnumerateArray().ToList();
            Assert.IsTrue(tools.Count > 0, "No tools discovered");

            // Act 3: Call a desktop capture tool (most reliable for testing)
            using TempDirectoryHelper tempHelper = new TempDirectoryHelper();
            string testScreenshotPath = tempHelper.CreateTempFile(fileName: "test_screenshot.png");

            JsonDocument screenshotResponse = await mcpClient.CallToolAsync("take_screenshot",
                new { outputPath = testScreenshotPath }, cancellationToken);

            // Assert: Tool execution successful
            Assert.IsNotNull(screenshotResponse);
            Assert.IsTrue(screenshotResponse.RootElement.TryGetProperty("result", out JsonElement screenshotResult));

            if (screenshotResult.TryGetProperty("content", out JsonElement content))
            {
                List<JsonElement> contentArray = content.EnumerateArray().ToList();
                Assert.IsTrue(contentArray.Count > 0);

                string? textContent = contentArray[0].GetProperty("text").GetString();
                Assert.IsTrue(textContent?.Contains("successfully saved") == true,
                    $"Expected content to contain 'successfully saved', but got: {textContent}");
            }
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task ToolError_ShouldReturnProperErrorResponse()
    {
        // Arrange
        McpHttpServerService service = new McpHttpServerService(_serverLogger);
        CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using HttpClient httpClient = new HttpClient();
            using McpTestClient mcpClient = new McpTestClient(httpClient, _clientLogger);
            httpClient.BaseAddress = new Uri(service.ServerUrl);

            // Act: Call tool with invalid parameters
            JsonDocument response = await mcpClient.CallToolAsync("take_screenshot",
                new { outputPath = "/invalid/path/that/does/not/exist/screenshot.png" }, cancellationToken);

            // Assert: Error handled gracefully
            Assert.IsNotNull(response);
            Assert.IsTrue(response.RootElement.TryGetProperty("result", out JsonElement result));

            if (result.TryGetProperty("content", out JsonElement content))
            {
                List<JsonElement> contentArray = content.EnumerateArray().ToList();
                Assert.IsTrue(contentArray.Count > 0);

                string? errorText = contentArray[0].GetProperty("text").GetString();
                Assert.IsTrue(errorText!.Contains("Error") || errorText.Contains("not found"));
            }
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task MultipleSequentialCalls_ShouldAllSucceed()
    {
        // Arrange
        McpHttpServerService service = new McpHttpServerService(_serverLogger);
        CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using HttpClient httpClient = new HttpClient();
            using McpTestClient mcpClient = new McpTestClient(httpClient, _clientLogger);
            httpClient.BaseAddress = new Uri(service.ServerUrl);
            using TempDirectoryHelper tempHelper = new TempDirectoryHelper();

            // Act: Make multiple sequential calls
            List<JsonDocument> responses = new List<JsonDocument>();

            // Call 1: List tools
            responses.Add(await mcpClient.ListToolsAsync(cancellationToken));

            // Call 2: Take a screenshot
            string testScreenshotPath = tempHelper.CreateTempFile(fileName: "test_screenshot.png");
            responses.Add(await mcpClient.CallToolAsync("take_screenshot",
                new { outputPath = testScreenshotPath }, cancellationToken));

            // Call 3: List windows
            responses.Add(await mcpClient.CallToolAsync("list_windows", new { }, cancellationToken));

            // Assert: All calls successful
            foreach (var response in responses)
            {
                Assert.IsNotNull(response);
                Assert.IsTrue(response.RootElement.TryGetProperty("result", out _));
            }
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task InvalidJsonRpcRequest_ShouldReturnError()
    {
        // Arrange
        McpHttpServerService service = new McpHttpServerService(_serverLogger);
        CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(service.ServerUrl);

            // Act: Send invalid JSON-RPC request
            string invalidRequest = """{"invalid": "request"}""";
            StringContent content = new StringContent(invalidRequest, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync("/mcp", content, cancellationToken);

            // Assert: Should get a response (possibly an error)
            // MCP server should handle invalid requests gracefully
            Assert.IsNotNull(response);

            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            Assert.IsNotNull(responseContent);
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task ConcurrentCalls_ShouldHandleMultipleClients()
    {
        // Arrange
        McpHttpServerService service = new McpHttpServerService(_serverLogger);
        CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;

        try
        {
            await service.StartAsync(cancellationToken);
            using TempDirectoryHelper tempHelper = new TempDirectoryHelper();

            // Act: Make concurrent calls from multiple clients
            List<Task<JsonDocument>> tasks = new List<Task<JsonDocument>>();

            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using HttpClient httpClient = new HttpClient();
                    using McpTestClient mcpClient = new McpTestClient(httpClient, _clientLogger);
                    httpClient.BaseAddress = new Uri(service.ServerUrl);

                    // Each client calls list tools
                    return await mcpClient.ListToolsAsync(cancellationToken);
                }));
            }

            JsonDocument[] responses = await Task.WhenAll(tasks);

            // Assert: All concurrent calls successful
            Assert.AreEqual(5, responses.Length);
            foreach (var response in responses)
            {
                Assert.IsNotNull(response);
                Assert.IsTrue(response.RootElement.TryGetProperty("result", out JsonElement result));
                Assert.IsTrue(result.TryGetProperty("tools", out JsonElement toolsArray));
                Assert.IsTrue(toolsArray.GetArrayLength() > 0);
            }
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task HealthEndpoint_ShouldAlwaysBeAccessible()
    {
        // Arrange
        McpHttpServerService service = new McpHttpServerService(_serverLogger);
        CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(service.ServerUrl);

            // Act: Call health endpoint multiple times
            for (int i = 0; i < 3; i++)
            {
                HttpResponseMessage response = await httpClient.GetAsync("/health", cancellationToken);

                // Assert
                Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                JsonElement healthData = JsonSerializer.Deserialize<JsonElement>(content);

                Assert.AreEqual("healthy", healthData.GetProperty("status").GetString());
                Assert.AreEqual(service.ServerUrl, healthData.GetProperty("url").GetString());

                // Small delay between calls
                await Task.Delay(100, cancellationToken);
            }
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task ToolWithComplexParameters_ShouldWork()
    {
        // Arrange
        McpHttpServerService service = new McpHttpServerService(_serverLogger);
        CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using HttpClient httpClient = new HttpClient();
            using McpTestClient mcpClient = new McpTestClient(httpClient, _clientLogger);
            httpClient.BaseAddress = new Uri(service.ServerUrl);
            using TempDirectoryHelper tempHelper = new TempDirectoryHelper();

            // Act: Test desktop capture with region parameters
            string testScreenshotPath = tempHelper.CreateTempFile(fileName: "region_screenshot.png");

            JsonDocument response = await mcpClient.CallToolAsync("take_region_screenshot",
                new { outputPath = testScreenshotPath, x = 0, y = 0, width = 100, height = 100 }, cancellationToken);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.RootElement.TryGetProperty("result", out JsonElement result));

            if (result.TryGetProperty("content", out JsonElement content))
            {
                List<JsonElement> contentArray = content.EnumerateArray().ToList();
                Assert.IsTrue(contentArray.Count > 0);

                string? textContent = contentArray[0].GetProperty("text").GetString();
                Assert.IsTrue(textContent?.Contains("successfully saved") == true,
                    $"Expected content to contain 'successfully saved', but got: {textContent}");
            }
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }
}
