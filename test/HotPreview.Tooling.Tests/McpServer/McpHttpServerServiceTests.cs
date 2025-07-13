using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Text.Json;
using HotPreview.Tooling.McpServer;
using HotPreview.Tooling.Tests.McpServer.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HotPreview.Tooling.Tests.McpServer;

[TestClass]
public class McpHttpServerServiceTests
{
    private ILogger<McpHttpServerService> _logger = null!;
    private ILogger<McpTestClient> _clientLogger = null!;

    [TestInitialize]
    public void Setup()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<McpHttpServerService>();
        _clientLogger = loggerFactory.CreateLogger<McpTestClient>();
    }

    [TestMethod]
    public async Task StartAsync_ShouldStartServerSuccessfully()
    {
        // Arrange
        var service = new McpHttpServerService(_logger);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            // Act
            await service.StartAsync(cancellationToken);

            // Assert
            Assert.IsTrue(!string.IsNullOrEmpty(service.ServerUrl));
            Assert.IsTrue(service.ServerUrl.StartsWith("http://localhost:"));

            // Verify server is accessible
            using var client = new HttpClient();
            var response = await client.GetAsync($"{service.ServerUrl}/health", cancellationToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var healthResponse = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.AreEqual("healthy", healthResponse.GetProperty("status").GetString());
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task StopAsync_ShouldStopServerGracefully()
    {
        // Arrange
        var service = new McpHttpServerService(_logger);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        await service.StartAsync(cancellationToken);
        var serverUrl = service.ServerUrl;

        // Act
        await service.StopAsync(cancellationToken);

        // Assert - server should no longer be accessible
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(2);
        
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () =>
        {
            await client.GetAsync($"{serverUrl}/health", cancellationToken);
        });
    }

    [TestMethod]
    public async Task ServerUrl_ShouldBeValidHttpUrl()
    {
        // Arrange
        var service = new McpHttpServerService(_logger);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            // Act
            await service.StartAsync(cancellationToken);

            // Assert
            Assert.IsTrue(Uri.TryCreate(service.ServerUrl, UriKind.Absolute, out var uri));
            Assert.AreEqual("http", uri!.Scheme);
            Assert.AreEqual("localhost", uri.Host);
            Assert.IsTrue(uri.Port > 0);
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task MultipleStartCalls_ShouldThrowException()
    {
        // Arrange
        var service = new McpHttpServerService(_logger);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await service.StartAsync(cancellationToken);
            });
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task McpEndpoint_ShouldBeAccessible()
    {
        // Arrange
        var service = new McpHttpServerService(_logger);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            // Act
            using var httpClient = new HttpClient();
            using var mcpClient = new McpTestClient(httpClient, _clientLogger);
            
            httpClient.BaseAddress = new Uri(service.ServerUrl);
            
            var toolsResponse = await mcpClient.ListToolsAsync(cancellationToken);

            // Assert
            Assert.IsNotNull(toolsResponse);
            Assert.IsTrue(toolsResponse.RootElement.TryGetProperty("result", out var result));
            Assert.IsTrue(result.TryGetProperty("tools", out var tools));
            Assert.IsTrue(tools.GetArrayLength() > 0);
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task HostedServiceIntegration_ShouldWorkWithDependencyInjection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IHostedService, McpHttpServerService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var hostedService = serviceProvider.GetRequiredService<IHostedService>();
        var mcpService = (McpHttpServerService)hostedService;
        
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            // Act
            await hostedService.StartAsync(cancellationToken);

            // Assert
            Assert.IsNotNull(mcpService.ServerUrl);
            
            using var client = new HttpClient();
            var response = await client.GetAsync($"{mcpService.ServerUrl}/health", cancellationToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            await hostedService.StopAsync(cancellationToken);
            await serviceProvider.DisposeAsync();
        }
    }
}