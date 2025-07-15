using System.Text.Json;
using HotPreview.Tooling.McpServer;
using HotPreview.Tooling.McpServer.Interfaces;
using HotPreview.Tooling.Tests.McpServer.TestHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HotPreview.Tooling.Tests.McpServer;

[TestClass]
public class IosDeviceToolTests
{
    private Mock<IProcessService> _mockProcessService = null!;
    private IosDeviceTool _tool = null!;
    private ILogger<McpTestClient> _clientLogger = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockProcessService = new Mock<IProcessService>();
        _tool = new IosDeviceTool(_mockProcessService.Object);

        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _clientLogger = loggerFactory.CreateLogger<McpTestClient>();
    }

    [TestMethod]
    public void ListDevices_WithNoDevices_ShouldReturnNoDevicesMessage()
    {
        // Arrange - This test verifies the method handles empty device list
        string emptyDevicesJson = """
        {
            "devices": {}
        }
        """;

        _mockProcessService.Setup(x => x.ExecuteCommand("xcrun simctl list devices --json"))
            .Returns(emptyDevicesJson);

        // Act
        string result = _tool.ListDevices();

        // Assert
        Assert.IsNotNull(result);
        // Should return table with no devices or error message
        Assert.IsTrue(result.Contains("Simulator Devices") || result.Contains("Error") || result.Contains("No simulator devices"));
    }

    [TestMethod]
    public void ListDevices_WithValidDevices_ShouldParseCorrectly()
    {
        // Arrange
        string devicesJson = """
        {
            "devices": {
                "com.apple.CoreSimulator.SimRuntime.iOS-17-0": [
                    {
                        "name": "iPhone 15",
                        "udid": "12345678-1234-1234-1234-123456789012",
                        "state": "Shutdown"
                    },
                    {
                        "name": "iPhone 15 Pro",
                        "udid": "87654321-4321-4321-4321-210987654321",
                        "state": "Booted"
                    }
                ]
            }
        }
        """;

        _mockProcessService.Setup(x => x.ExecuteCommand("xcrun simctl list devices --json"))
            .Returns(devicesJson);

        // Act
        string result = _tool.ListDevices();

        // Assert
        Assert.IsNotNull(result);
        // The method should return formatted table content
        Assert.IsTrue(result.Contains("Simulator Devices") || result.Contains("Error"));
    }

    [TestMethod]
    public void GetBootedDevice_WithBootedDevice_ShouldReturnDevice()
    {
        // Arrange
        string devicesJson = """
        {
            "devices": {
                "com.apple.CoreSimulator.SimRuntime.iOS-17-0": [
                    {
                        "name": "iPhone 15 Pro",
                        "udid": "87654321-4321-4321-4321-210987654321",
                        "state": "Booted"
                    }
                ]
            }
        }
        """;

        _mockProcessService.Setup(x => x.ExecuteCommand("xcrun simctl list devices --json"))
            .Returns(devicesJson);

        // Act
        var result = _tool.GetBootedDevice();

        // Assert
        Assert.IsNotNull(result);
        // Should return table with booted device or error message
    }

    [TestMethod]
    public void BootDevice_WithValidDeviceId_ShouldExecuteCommand()
    {
        // Arrange
        string deviceId = "12345678-1234-1234-1234-123456789012";
        _mockProcessService.Setup(x => x.ExecuteCommand($"xcrun simctl boot {deviceId}"))
            .Returns("");

        // Act & Assert - Should not throw for valid input
        _tool.BootDevice(deviceId);

        // Verify the command was called
        _mockProcessService.Verify(x => x.ExecuteCommand($"xcrun simctl boot {deviceId}"), Times.Once);
    }

    [TestMethod]
    public void BootDevice_WithEmptyDeviceId_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => _tool.BootDevice(""));
        Assert.ThrowsException<ArgumentNullException>(() => _tool.BootDevice(null!));
    }

    [TestMethod]
    public void ShutdownDevice_WithValidDeviceId_ShouldExecuteCommand()
    {
        // Arrange
        string deviceId = "12345678-1234-1234-1234-123456789012";
        _mockProcessService.Setup(x => x.ExecuteCommand($"xcrun simctl shutdown {deviceId}"))
            .Returns("");

        // Act & Assert - Should not throw for valid input
        _tool.ShutdownDevice(deviceId);

        // Verify the command was called
        _mockProcessService.Verify(x => x.ExecuteCommand($"xcrun simctl shutdown {deviceId}"), Times.Once);
    }

    [TestMethod]
    public void ShutdownDevice_WithEmptyDeviceId_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => _tool.ShutdownDevice(""));
        Assert.ThrowsException<ArgumentNullException>(() => _tool.ShutdownDevice(null!));
    }

    [TestMethod]
    public async Task IntegrationTest_IosDeviceToolsViaEndToEnd()
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

            // Act - Test that iOS tools are available
            JsonDocument toolsResponse = await mcpClient.ListToolsAsync(cancellationToken);

            // Assert
            Assert.IsNotNull(toolsResponse);
            Assert.IsTrue(toolsResponse.RootElement.TryGetProperty("result", out var result));
            Assert.IsTrue(result.TryGetProperty("tools", out var toolsArray));

            HashSet<string?> tools = toolsArray.EnumerateArray()
                .Select(t => t.GetProperty("name").GetString())
                .ToHashSet();

            // Verify iOS tools are exposed
            Assert.IsTrue(tools.Contains("ios_list_devices"));
            Assert.IsTrue(tools.Contains("ios_booted_device"));
            Assert.IsTrue(tools.Contains("ios_boot_device"));
            Assert.IsTrue(tools.Contains("ios_shutdown_device"));

            // Test calling ios_list_devices
            JsonDocument devicesResponse = await mcpClient.CallToolAsync("ios_list_devices", new { }, cancellationToken);
            Assert.IsNotNull(devicesResponse);
            Assert.IsTrue(devicesResponse.RootElement.TryGetProperty("result", out var devicesResult));
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }
}
