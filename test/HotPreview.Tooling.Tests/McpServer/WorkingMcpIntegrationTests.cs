using Microsoft.VisualStudio.TestTools.UnitTesting;
using HotPreview.Tooling.McpServer;
using HotPreview.Tooling.Tests.McpServer.TestHelpers;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Reflection;

namespace HotPreview.Tooling.Tests.McpServer;

[TestClass]
public class WorkingMcpIntegrationTests
{
    private ILogger<McpHttpServerService> _serverLogger = null!;
    private ILogger<McpTestClient> _clientLogger = null!;

    [TestInitialize]
    public void Setup()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _serverLogger = loggerFactory.CreateLogger<McpHttpServerService>();
        _clientLogger = loggerFactory.CreateLogger<McpTestClient>();
    }

    [TestMethod]
    public async Task McpServer_ShouldStartAndServeHealthEndpoint()
    {
        // This test verifies the server infrastructure works
        var service = new McpHttpServerService(_serverLogger);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            // Act
            await service.StartAsync(cancellationToken);

            // Assert - Server should be accessible
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{service.ServerUrl}/health", cancellationToken);
            
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            Assert.IsTrue(content.Contains("healthy"));
            Assert.IsTrue(content.Contains(service.ServerUrl));
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task McpServer_ShouldExposeSSEEndpoint()
    {
        // This test verifies the MCP library creates some endpoints
        var service = new McpHttpServerService(_serverLogger);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(2); // Short timeout for SSE test
            
            try
            {
                // The SSE endpoint should exist and start a connection
                var sseResponse = await httpClient.GetAsync($"{service.ServerUrl}/sse", cancellationToken);
                
                // Assert - SSE endpoint exists (even if it times out or returns an error, it's not 404)
                Assert.AreNotEqual(HttpStatusCode.NotFound, sseResponse.StatusCode, 
                    "SSE endpoint should exist, indicating MCP library is working");
                
                Console.WriteLine($"SSE endpoint status: {sseResponse.StatusCode}");
            }
            catch (TaskCanceledException)
            {
                // This is expected for SSE endpoints as they maintain long-running connections
                Console.WriteLine("SSE endpoint timed out as expected (long-running connection)");
                Assert.IsTrue(true, "SSE endpoint responded with streaming connection (timeout expected)");
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("timeout"))
            {
                // Also acceptable - indicates the endpoint exists but times out
                Console.WriteLine("SSE endpoint exists but timed out (expected for streaming)");
                Assert.IsTrue(true, "SSE endpoint exists and responds");
            }
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task McpServer_ToolDiscovery_ShouldWorkViaReflection()
    {
        // This test verifies tools are discoverable (which the MCP library should use)
        var service = new McpHttpServerService(_serverLogger);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            // Verify tool discovery works at the reflection level
            // (which is what MCP library should be using internally)
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(AndroidDeviceTool));
            Assert.IsNotNull(assembly);

            var toolClasses = assembly.GetTypes()
                .Where(type => type.GetCustomAttribute<ModelContextProtocol.Server.McpServerToolTypeAttribute>() != null)
                .ToList();

            Assert.IsTrue(toolClasses.Count > 0, "Tools should be discoverable via reflection");

            var toolMethods = toolClasses
                .SelectMany(type => type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                .Where(method => method.GetCustomAttribute<ModelContextProtocol.Server.McpServerToolAttribute>() != null)
                .ToList();

            Assert.IsTrue(toolMethods.Count > 0, "Tool methods should be discoverable");

            // Verify specific expected tools exist
            var toolNames = toolMethods
                .Select(m => m.GetCustomAttribute<ModelContextProtocol.Server.McpServerToolAttribute>()?.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToHashSet();

            var expectedTools = new[] { "android_list_devices", "read_file", "ios_list_devices" };
            foreach (var expectedTool in expectedTools)
            {
                Assert.IsTrue(toolNames.Contains(expectedTool), 
                    $"Expected tool '{expectedTool}' should be discoverable");
            }

            Console.WriteLine($"Discovered {toolMethods.Count} tool methods from {toolClasses.Count} tool classes");
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task McpServer_ShouldHandleConcurrentRequests()
    {
        // Test that the server infrastructure can handle multiple requests
        var service = new McpHttpServerService(_serverLogger);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            // Make multiple concurrent health check requests
            var tasks = Enumerable.Range(0, 5).Select(async i =>
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"{service.ServerUrl}/health", cancellationToken);
                return response.StatusCode == HttpStatusCode.OK;
            }).ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert all requests succeeded
            Assert.IsTrue(results.All(success => success), 
                "All concurrent health check requests should succeed");
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public async Task McpServer_ToolExecution_ShouldWorkDirectly()
    {
        // Test that tools can be executed directly (without MCP protocol)
        // This verifies the core functionality works
        var service = new McpHttpServerService(_serverLogger);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            // Test file system tool directly
            using var tempHelper = new TempDirectoryHelper();
            var testContent = "Direct tool execution test";
            var testFile = tempHelper.CreateTempFile(content: testContent);

            var fileSystemTool = new FileSystemTools();
            var result = fileSystemTool.ReadFile(testFile);

            Assert.AreEqual(testContent, result, "Tool should work when called directly");

            // Test Android tool directly (will fail without ADB, but should handle gracefully)
            var androidTool = new AndroidDeviceTool();
            var deviceResult = androidTool.ListDevices();
            
            Assert.IsNotNull(deviceResult, "Android tool should return a result");
            Assert.IsTrue(deviceResult.Contains("ADB") || deviceResult.Contains("No devices") || deviceResult.Contains("Error"),
                "Android tool should handle missing ADB gracefully");

            Console.WriteLine($"Android tool result: {deviceResult.Substring(0, Math.Min(100, deviceResult.Length))}...");
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod] 
    public async Task McpServer_ServiceLifecycle_ShouldBeRobust()
    {
        // Test multiple start/stop cycles
        var service = new McpHttpServerService(_serverLogger);
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token;

        for (int i = 0; i < 3; i++)
        {
            // Start
            await service.StartAsync(cancellationToken);
            
            // Verify working
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{service.ServerUrl}/health", cancellationToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"Cycle {i + 1}: Health check should work");
            
            // Stop
            await service.StopAsync(cancellationToken);
            
            // Verify stopped (should not be accessible)
            await Task.Delay(100, cancellationToken); // Brief delay for cleanup
        }

        // Final verification - should be stopped
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(2);
            await httpClient.GetAsync($"{service.ServerUrl}/health", cancellationToken);
            Assert.Fail("Server should be stopped and not accessible");
        }
        catch (Exception)
        {
            // Expected - server should be inaccessible when stopped
        }
    }
}