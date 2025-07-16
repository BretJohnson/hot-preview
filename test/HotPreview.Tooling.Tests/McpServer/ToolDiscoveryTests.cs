using System.Reflection;
using System.Text.Json;
using HotPreview.Tooling.McpServer;
using HotPreview.Tooling.McpServer.Tools.Android;
using HotPreview.Tooling.Tests.McpServer.TestHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol.Server;

namespace HotPreview.Tooling.Tests.McpServer;

[TestClass]
public class ToolDiscoveryTests
{
    private ILogger<McpHttpServerService> _logger = null!;
    private ILogger<McpTestClient> _clientLogger = null!;

    [TestInitialize]
    public void Setup()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<McpHttpServerService>();
        _clientLogger = loggerFactory.CreateLogger<McpTestClient>();
    }

    [TestMethod]
    public void DiscoverToolClasses_ShouldFindAllToolClasses()
    {
        // Arrange
        Assembly? assembly = Assembly.GetAssembly(typeof(AndroidDeviceTool));
        Assert.IsNotNull(assembly);

        // Act
        List<Type> toolClasses = assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<McpServerToolTypeAttribute>() != null)
            .ToList();

        // Assert
        Assert.IsTrue(toolClasses.Count > 0, "No tool classes found with McpServerToolTypeAttribute");

        // Verify specific expected tool classes
        string[] expectedToolClasses =
        [
            "AndroidDeviceTool",
            "AndroidAppManagementTool",
            "AndroidUiTool",
            "AndroidDiagnosticsTool",
            "AndroidScreenshotTool",
            "IosDeviceTool",
            "IosUiTool",
            "IosScreenshotTool",
            "DesktopCaptureScreen"
        ];

        foreach (string expectedClass in expectedToolClasses)
        {
            Assert.IsTrue(toolClasses.Any(t => t.Name == expectedClass),
                $"Expected tool class {expectedClass} not found");
        }
    }

    [TestMethod]
    public void DiscoverToolMethods_ShouldFindAllToolMethods()
    {
        // Arrange
        Assembly? assembly = Assembly.GetAssembly(typeof(AndroidDeviceTool));
        Assert.IsNotNull(assembly);

        // Act
        List<MethodInfo> toolMethods = assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<McpServerToolTypeAttribute>() != null)
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            .Where(method => method.GetCustomAttribute<McpServerToolAttribute>() != null)
            .ToList();

        // Assert
        Assert.IsTrue(toolMethods.Count > 0, "No tool methods found with McpServerToolAttribute");

        // Verify each tool method has required attributes
        foreach (var method in toolMethods)
        {
            McpServerToolAttribute? toolAttribute = method.GetCustomAttribute<McpServerToolAttribute>();
            Assert.IsNotNull(toolAttribute, $"Method {method.Name} missing McpServerToolAttribute");
            Assert.IsFalse(string.IsNullOrEmpty(toolAttribute.Name),
                $"Method {method.Name} has empty tool name");

            System.ComponentModel.DescriptionAttribute? descriptionAttribute = method.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
            Assert.IsNotNull(descriptionAttribute,
                $"Method {method.Name} missing DescriptionAttribute");
            Assert.IsFalse(string.IsNullOrEmpty(descriptionAttribute.Description),
                $"Method {method.Name} has empty description");
        }
    }

    [TestMethod]
    public async Task McpServerToolDiscovery_ShouldExposeAllExpectedTools()
    {
        // Arrange
        McpHttpServerService service = new McpHttpServerService(_logger);
        CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;

        try
        {
            await service.StartAsync(cancellationToken);

            using HttpClient httpClient = new HttpClient();
            using McpTestClient mcpClient = new McpTestClient(httpClient, _clientLogger);
            httpClient.BaseAddress = new Uri(service.ServerUrl);

            // Act
            JsonDocument toolsResponse = await mcpClient.ListToolsAsync(cancellationToken);

            // Assert
            Assert.IsNotNull(toolsResponse);
            Assert.IsTrue(toolsResponse.RootElement.TryGetProperty("result", out var result));
            Assert.IsTrue(result.TryGetProperty("tools", out var toolsArray));

            List<JsonElement> tools = toolsArray.EnumerateArray().ToList();
            Assert.IsTrue(tools.Count > 0, "No tools discovered by MCP server");

            // Verify specific expected tools
            string[] expectedTools =
            [
                "android_list_devices",
                "android_boot_device",
                "android_shutdown_device",
                "android_execute_adb",
                "android_install_app",
                "android_launch_app",
                "android_ui_tap",
                "android_ui_swipe",
                "android_screenshot",
                "ios_list_devices",
                "ios_boot_device",
                "ios_screenshot",
                "take_screenshot",
                "take_region_screenshot",
                "take_application_screenshot",
                "list_windows"
            ];

            HashSet<string?> discoveredToolNames = tools
                .Select(t => t.GetProperty("name").GetString())
                .ToHashSet();

            foreach (string expectedTool in expectedTools)
            {
                Assert.IsTrue(discoveredToolNames.Contains(expectedTool),
                    $"Expected tool '{expectedTool}' not found in discovered tools. " +
                    $"Available tools: {string.Join(", ", discoveredToolNames)}");
            }

            // Verify tool structure
            foreach (var tool in tools)
            {
                Assert.IsTrue(tool.TryGetProperty("name", out var name));
                Assert.IsFalse(string.IsNullOrEmpty(name.GetString()));

                Assert.IsTrue(tool.TryGetProperty("description", out var description));
                Assert.IsFalse(string.IsNullOrEmpty(description.GetString()));

                Assert.IsTrue(tool.TryGetProperty("inputSchema", out var inputSchema));
                Assert.IsNotNull(inputSchema);
            }
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }

    [TestMethod]
    public void ValidateToolMethodSignatures_ShouldHaveCompatibleParameterTypes()
    {
        // Arrange
        Assembly? assembly = Assembly.GetAssembly(typeof(AndroidDeviceTool));
        Assert.IsNotNull(assembly);

        // Act
        List<MethodInfo> toolMethods = assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<McpServerToolTypeAttribute>() != null)
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            .Where(method => method.GetCustomAttribute<McpServerToolAttribute>() != null)
            .ToList();

        // Assert
        foreach (var method in toolMethods)
        {
            ParameterInfo[] parameters = method.GetParameters();

            // Verify parameter types are MCP-compatible
            foreach (var param in parameters)
            {
                Type paramType = param.ParameterType;

                // Check if type is supported by MCP
                Assert.IsTrue(IsMcpCompatibleType(paramType),
                    $"Method {method.DeclaringType?.Name}.{method.Name} has incompatible parameter type {paramType.Name}");
            }

            // Verify return type is MCP-compatible
            Assert.IsTrue(IsMcpCompatibleReturnType(method.ReturnType),
                $"Method {method.DeclaringType?.Name}.{method.Name} has incompatible return type {method.ReturnType.Name}");
        }
    }

    private static bool IsMcpCompatibleType(Type type)
    {
        // Unwrap nullable types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = Nullable.GetUnderlyingType(type)!;
        }

        // MCP supports these primitive types and string
        return type == typeof(string) ||
               type == typeof(int) ||
               type == typeof(long) ||
               type == typeof(float) ||
               type == typeof(double) ||
               type == typeof(bool) ||
               type == typeof(decimal) ||
               type == typeof(byte[]) ||  // Support for binary data (e.g., screenshots)
               type.IsEnum;
    }

    private static bool IsMcpCompatibleReturnType(Type type)
    {
        if (type == typeof(void) || type == typeof(Task))
            return true;

        // Unwrap Task<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            type = type.GetGenericArguments()[0];
        }

        return IsMcpCompatibleType(type);
    }
}
