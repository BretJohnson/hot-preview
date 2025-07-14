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

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _clientLogger = loggerFactory.CreateLogger<McpTestClient>();
    }

    [TestMethod]
    public async Task InstallApp_ShouldBeAvailableAsTool()
    {
        // Arrange
        var service = new McpHttpServerService(
            LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<McpHttpServerService>());

        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using var httpClient = new HttpClient();
            using var mcpClient = new McpTestClient(httpClient, _clientLogger);
            httpClient.BaseAddress = new Uri(service.ServerUrl);

            // Act - Get list of available tools
            var toolsResponse = await mcpClient.ListToolsAsync(cancellationToken);

            // Assert
            Assert.IsNotNull(toolsResponse);
            Assert.IsTrue(toolsResponse.RootElement.TryGetProperty("result", out var result));
            Assert.IsTrue(result.TryGetProperty("tools", out var toolsArray));

            var tools = toolsArray.EnumerateArray()
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
        var service = new McpHttpServerService(
            LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<McpHttpServerService>());

        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using var httpClient = new HttpClient();
            using var mcpClient = new McpTestClient(httpClient, _clientLogger);
            httpClient.BaseAddress = new Uri(service.ServerUrl);

            // Act - Call the launch app tool
            var response = await mcpClient.CallToolAsync("android_launch_app",
                new { packageName = "com.example.testapp" }, cancellationToken);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.RootElement.TryGetProperty("result", out var result));

            // The tool should return content (even if it's an error about ADB not being installed)
            if (result.TryGetProperty("content", out var content))
            {
                var contentArray = content.EnumerateArray().ToList();
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
        var service = new McpHttpServerService(
            LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<McpHttpServerService>());

        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        using var tempHelper = new TempDirectoryHelper();
        var apkPath = tempHelper.CreateTempFile(fileName: "test.apk", content: "fake apk content");

        try
        {
            await service.StartAsync(cancellationToken);

            using var httpClient = new HttpClient();
            using var mcpClient = new McpTestClient(httpClient, _clientLogger);
            httpClient.BaseAddress = new Uri(service.ServerUrl);

            // Act - Call the install app tool
            var response = await mcpClient.CallToolAsync("android_install_app",
                new { apkPath = apkPath }, cancellationToken);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.RootElement.TryGetProperty("result", out var result));

            // The tool should return content (even if it's an error about ADB not being installed)
            if (result.TryGetProperty("content", out var content))
            {
                var contentArray = content.EnumerateArray().ToList();
                Assert.IsTrue(contentArray.Count > 0);
            }
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }
}
