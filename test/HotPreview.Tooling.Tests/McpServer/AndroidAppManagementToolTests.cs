using System.Text.Json;
using HotPreview.Tooling.McpServer;
using HotPreview.Tooling.Tests.McpServer.TestHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HotPreview.Tooling.Tests.McpServer;

[TestClass]
public class AndroidAppManagementToolTests
{
    private MockCommandExecutor _mockExecutor = null!;
    private ILogger<McpTestClient> _clientLogger = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockExecutor = new MockCommandExecutor();

        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _clientLogger = loggerFactory.CreateLogger<McpTestClient>();
    }

    [TestMethod]
    public async Task InstallApp_ShouldBeAvailableAsTool()
    {
        // Arrange
        McpHttpServerService service = new McpHttpServerService(
            LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<McpHttpServerService>());

        CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using HttpClient httpClient = new HttpClient();
            using McpTestClient mcpClient = new McpTestClient(httpClient, _clientLogger);
            httpClient.BaseAddress = new Uri(service.ServerUrl);

            // Act - Get list of available tools
            JsonDocument toolsResponse = await mcpClient.ListToolsAsync(cancellationToken);

            // Assert
            Assert.IsNotNull(toolsResponse);
            Assert.IsTrue(toolsResponse.RootElement.TryGetProperty("result", out var result));
            Assert.IsTrue(result.TryGetProperty("tools", out var toolsArray));

            List<string?> tools = toolsArray.EnumerateArray()
                .Select(t => t.GetProperty("name").GetString())
                .ToList();

            Assert.IsTrue(tools.Contains("android_install_app"));
            Assert.IsTrue(tools.Contains("android_launch_app"));
            Assert.IsTrue(tools.Contains("android_uninstall_app"));
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task LaunchApp_WithValidPackageName_ShouldCallTool()
    {
        // Arrange
        McpHttpServerService service = new McpHttpServerService(
            LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<McpHttpServerService>());

        CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using HttpClient httpClient = new HttpClient();
            using McpTestClient mcpClient = new McpTestClient(httpClient, _clientLogger);
            httpClient.BaseAddress = new Uri(service.ServerUrl);

            // Act - Call the launch app tool
            JsonDocument response = await mcpClient.CallToolAsync("android_launch_app",
                new { packageName = "com.example.testapp" }, cancellationToken);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.RootElement.TryGetProperty("result", out var result));

            // The tool should return content (even if it's an error about ADB not being installed)
            if (result.TryGetProperty("content", out var content))
            {
                List<JsonElement> contentArray = content.EnumerateArray().ToList();
                Assert.IsTrue(contentArray.Count > 0);
            }
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task InstallApp_WithApkPath_ShouldCallTool()
    {
        // Arrange
        McpHttpServerService service = new McpHttpServerService(
            LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<McpHttpServerService>());

        CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        using TempDirectoryHelper tempHelper = new TempDirectoryHelper();
        string apkPath = tempHelper.CreateTempFile(fileName: "test.apk", content: "fake apk content");

        try
        {
            await service.StartAsync(cancellationToken);

            using HttpClient httpClient = new HttpClient();
            using McpTestClient mcpClient = new McpTestClient(httpClient, _clientLogger);
            httpClient.BaseAddress = new Uri(service.ServerUrl);

            // Act - Call the install app tool
            JsonDocument response = await mcpClient.CallToolAsync("android_install_app",
                new { apkPath = apkPath }, cancellationToken);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.RootElement.TryGetProperty("result", out var result));

            // The tool should return content (even if it's an error about ADB not being installed)
            if (result.TryGetProperty("content", out var content))
            {
                List<JsonElement> contentArray = content.EnumerateArray().ToList();
                Assert.IsTrue(contentArray.Count > 0);
            }
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }
}
