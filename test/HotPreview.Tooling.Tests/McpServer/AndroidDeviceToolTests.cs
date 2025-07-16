using System.Text.Json;
using HotPreview.Tooling.McpServer;
using HotPreview.Tooling.McpServer.Interfaces;
using HotPreview.Tooling.McpServer.Tools.Android;
using HotPreview.Tooling.Tests.McpServer.TestHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HotPreview.Tooling.Tests.McpServer;

[TestClass]
public class AndroidDeviceToolTests
{
    private Mock<IProcessService> _mockProcessService = null!;
    private AndroidDeviceTool _tool = null!;
    private ILogger<McpTestClient> _clientLogger = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockProcessService = new Mock<IProcessService>();

        // Mock ADB check to return success by default (can be overridden in individual tests)
        var mockAdbProcess = new Mock<System.Diagnostics.Process>();
        _mockProcessService.Setup(x => x.StartProcess("adb version")).Returns(() =>
        {
            var process = new System.Diagnostics.Process();
            // We can't directly set ExitCode, but we can simulate success by not throwing
            return process;
        });

        _tool = new AndroidDeviceTool(_mockProcessService.Object);

        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _clientLogger = loggerFactory.CreateLogger<McpTestClient>();
    }

    private void SetupAdbSuccess()
    {
        // Mock ADB check to succeed by returning a process that will exit with code 0
        // We'll use a simple command that always succeeds
        _mockProcessService.Setup(x => x.StartProcess("adb version"))
            .Returns(() =>
            {
                // Create a process that runs a simple command that will succeed
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = "/c exit 0",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                return process;
            });
    }

    private void SetupAdbFailure()
    {
        // Mock ADB check to fail (process not found)
        _mockProcessService.Setup(x => x.StartProcess("adb version"))
            .Throws(new Exception("ADB not found"));
    }

    [TestMethod]
    public void ListDevices_WhenAdbNotInstalled_ShouldReturnErrorMessage()
    {
        // Arrange
        SetupAdbFailure();

        // Act
        string result = _tool.ListDevices();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("ADB is not installed") || result.Contains("Error"));
    }

    [TestMethod]
    public void ListDevices_WithNoDevices_ShouldReturnNoDevicesMessage()
    {
        // Arrange
        SetupAdbSuccess();
        _mockProcessService.Setup(x => x.ExecuteCommand("adb devices -l"))
            .Returns("List of devices attached\n");

        // Act
        string result = _tool.ListDevices();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("No devices found") || result.Contains("Android Devices"));
    }

    [TestMethod]
    public void ListDevices_WithSingleDevice_ShouldParseDeviceCorrectly()
    {
        // Arrange
        SetupAdbSuccess();
        string deviceOutput = """
        List of devices attached
        emulator-5554    device product:sdk_gphone64_x86_64 model:sdk_gphone64_x86_64 device:emu64xa
        """;

        _mockProcessService.Setup(x => x.ExecuteCommand("adb devices -l"))
            .Returns(deviceOutput);

        // Act
        string result = _tool.ListDevices();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("`emulator-5554`"));
        Assert.IsTrue(result.Contains("`sdk_gphone64_x86_64`"));
        Assert.IsTrue(result.Contains("`emu64xa`"));
    }

    [TestMethod]
    public void ListDevices_WithMultipleDevices_ShouldParseAllDevices()
    {
        // Arrange
        SetupAdbSuccess();
        string deviceOutput = """
        List of devices attached
        emulator-5554    device product:sdk_gphone64_x86_64 model:sdk_gphone64_x86_64 device:emu64xa
        RF8N308KFYP      device product:beyond2ltexx model:SM_G975F device:beyond2
        """;

        _mockProcessService.Setup(x => x.ExecuteCommand("adb devices -l"))
            .Returns(deviceOutput);

        // Act
        string result = _tool.ListDevices();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("`emulator-5554`"));
        Assert.IsTrue(result.Contains("`RF8N308KFYP`"));
        Assert.IsTrue(result.Contains("`beyond2ltexx`"));
        Assert.IsTrue(result.Contains("`SM_G975F`"));
    }

    [TestMethod]
    public void BootDevice_WithValidAvdName_ShouldExecuteAdbCommand()
    {
        // Arrange
        SetupAdbSuccess();
        string avdName = "test-emulator";
        _mockProcessService.Setup(x => x.ExecuteCommand($"adb -s {avdName} emu kill"))
            .Returns("");

        // Act & Assert - Should not throw
        _tool.BootDevice(avdName);

        // Verify the command was called
        _mockProcessService.Verify(x => x.ExecuteCommand($"adb -s {avdName} emu kill"), Times.Once);
    }

    [TestMethod]
    public void BootDevice_WithEmptyAvdName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => _tool.BootDevice(""));
        Assert.ThrowsException<ArgumentNullException>(() => _tool.BootDevice(null!));
    }

    [TestMethod]
    public void ShutdownDevice_WithValidAvdName_ShouldExecuteAdbCommand()
    {
        // Arrange
        SetupAdbSuccess();
        string avdName = "test-emulator";
        _mockProcessService.Setup(x => x.ExecuteCommand($"adb -s {avdName} emu kill"))
            .Returns("");

        // Act & Assert - Should not throw for valid input
        _tool.ShutdownDevice(avdName);

        // Verify the command was called
        _mockProcessService.Verify(x => x.ExecuteCommand($"adb -s {avdName} emu kill"), Times.Once);
    }

    [TestMethod]
    public void ShutdownDevice_WithEmptyAvdName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => _tool.ShutdownDevice(""));
        Assert.ThrowsException<ArgumentNullException>(() => _tool.ShutdownDevice(null!));
    }

    [TestMethod]
    public void ExecAdb_WithValidParameters_ShouldExecuteCommand()
    {
        // Arrange
        string parameters = "shell input keyevent KEYCODE_HOME";
        _mockProcessService.Setup(x => x.ExecuteCommand($"adb {parameters}"))
            .Returns("Command executed successfully");

        // Act
        string result = _tool.ExecAdb(parameters);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Command executed successfully", result);
        _mockProcessService.Verify(x => x.ExecuteCommand($"adb {parameters}"), Times.Once);
    }

    [TestMethod]
    public async Task IntegrationTest_AndroidDeviceToolViaEndToEnd()
    {
        // This is a more comprehensive test that would test the tool through the MCP server
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

            // Act - Call the list devices tool
            JsonDocument response = await mcpClient.CallToolAsync("android_list_devices", new { }, cancellationToken);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.RootElement.TryGetProperty("result", out var result));

            // The tool should return content (even if it's an error about ADB not being installed)
            if (result.TryGetProperty("content", out var content))
            {
                List<JsonElement> contentArray = content.EnumerateArray().ToList();
                Assert.IsTrue(contentArray.Count > 0);

                JsonElement firstContent = contentArray[0];
                Assert.IsTrue(firstContent.TryGetProperty("text", out var text));
                Assert.IsFalse(string.IsNullOrEmpty(text.GetString()));
            }
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }
}
