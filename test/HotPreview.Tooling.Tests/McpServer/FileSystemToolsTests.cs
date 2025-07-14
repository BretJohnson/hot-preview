using System.Text.Json;
using HotPreview.Tooling.McpServer;
using HotPreview.Tooling.Tests.McpServer.TestHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HotPreview.Tooling.Tests.McpServer;

[TestClass]
public class FileSystemToolsTests
{
    private FileSystemTools _tool = null!;
    private TempDirectoryHelper _tempHelper = null!;
    private ILogger<McpTestClient> _clientLogger = null!;

    [TestInitialize]
    public void Setup()
    {
        _tool = new FileSystemTools();
        _tempHelper = new TempDirectoryHelper();

        ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _clientLogger = loggerFactory.CreateLogger<McpTestClient>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _tempHelper?.Dispose();
    }

    [TestMethod]
    public void ReadFile_WithExistingFile_ShouldReturnContent()
    {
        // Arrange
        string content = "Hello, World!\nThis is a test file.";
        string filePath = _tempHelper.CreateTempFile(content: content);

        // Act
        string result = _tool.ReadFile(filePath);

        // Assert
        Assert.AreEqual(content, result);
    }

    [TestMethod]
    public void ReadFile_WithNonExistentFile_ShouldReturnError()
    {
        // Arrange
        string nonExistentPath = Path.Combine(_tempHelper.CreateTempDirectory(), "nonexistent.txt");

        // Act
        string result = _tool.ReadFile(nonExistentPath);

        // Assert
        Assert.IsTrue(result.StartsWith("Error: File not found"));
        Assert.IsTrue(result.Contains(nonExistentPath));
    }

    [TestMethod]
    public void WriteFile_ShouldCreateFileWithContent()
    {
        // Arrange
        string content = "Test content to write";
        string filePath = Path.Combine(_tempHelper.CreateTempDirectory(), "test.txt");

        // Act
        string result = _tool.WriteFile(filePath, content);

        // Assert
        Assert.IsTrue(result.StartsWith("Successfully wrote content"));
        Assert.IsTrue(File.Exists(filePath));
        Assert.AreEqual(content, File.ReadAllText(filePath));
    }

    [TestMethod]
    public void ReadBinaryFile_WithExistingFile_ShouldReturnBase64()
    {
        // Arrange
        byte[] binaryData = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello" in bytes
        string filePath = Path.Combine(_tempHelper.CreateTempDirectory(), "test.bin");
        File.WriteAllBytes(filePath, binaryData);

        // Act
        string result = _tool.ReadBinaryFile(filePath);

        // Assert
        string expectedBase64 = Convert.ToBase64String(binaryData);
        Assert.AreEqual(expectedBase64, result);
    }

    [TestMethod]
    public void WriteBinaryFile_ShouldCreateFileFromBase64()
    {
        // Arrange
        byte[] binaryData = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello" in bytes
        string base64Content = Convert.ToBase64String(binaryData);
        string filePath = Path.Combine(_tempHelper.CreateTempDirectory(), "test.bin");

        // Act
        string result = _tool.WriteBinaryFile(filePath, base64Content);

        // Assert
        Assert.IsTrue(result.StartsWith("Successfully wrote binary content"));
        Assert.IsTrue(File.Exists(filePath));
        CollectionAssert.AreEqual(binaryData, File.ReadAllBytes(filePath));
    }

    [TestMethod]
    public void ReadFileLines_WithValidRange_ShouldReturnSpecificLines()
    {
        // Arrange
        string[] lines = new[] { "Line 1", "Line 2", "Line 3", "Line 4", "Line 5" };
        string content = string.Join(Environment.NewLine, lines);
        string filePath = _tempHelper.CreateTempFile(content: content);

        // Act
        string result = _tool.ReadFileLines(filePath, 2, 4);

        // Assert
        string expectedContent = string.Join(Environment.NewLine, lines[1..4]);
        Assert.AreEqual(expectedContent, result);
    }

    [TestMethod]
    public void ReadFileLines_WithInvalidRange_ShouldReturnError()
    {
        // Arrange
        string filePath = _tempHelper.CreateTempFile(content: "Line 1\nLine 2");

        // Act & Assert
        string result1 = _tool.ReadFileLines(filePath, 0, 2);
        Assert.IsTrue(result1.StartsWith("Error: Start line must be at least 1"));

        string result2 = _tool.ReadFileLines(filePath, 3, 1);
        Assert.IsTrue(result2.StartsWith("Error: End line must be greater than"));
    }

    [TestMethod]
    public void WriteFileLines_ShouldReplaceSpecificLines()
    {
        // Arrange
        string initialContent = "Line 1\nLine 2\nLine 3\nLine 4";
        string filePath = _tempHelper.CreateTempFile(content: initialContent);
        string newContent = "New Line 2\nNew Line 3";

        // Act
        string result = _tool.WriteFileLines(filePath, newContent, 2);

        // Assert
        Assert.IsTrue(result.StartsWith("Successfully wrote content"));
        string fileContent = File.ReadAllText(filePath);
        string expectedContent = "Line 1\nNew Line 2\nNew Line 3\nLine 4";
        Assert.AreEqual(expectedContent, fileContent);
    }

    [TestMethod]
    public void InsertFileLines_ShouldInsertContentAtSpecificLine()
    {
        // Arrange
        string initialContent = "Line 1\nLine 2\nLine 3";
        string filePath = _tempHelper.CreateTempFile(content: initialContent);
        string insertContent = "Inserted Line";

        // Act
        string result = _tool.InsertFileLines(filePath, insertContent, 2);

        // Assert
        Assert.IsTrue(result.StartsWith("Successfully inserted content"));
        string fileContent = File.ReadAllText(filePath);
        string expectedContent = "Line 1\nInserted Line\nLine 2\nLine 3";
        Assert.AreEqual(expectedContent, fileContent);
    }

    [TestMethod]
    public void ListDirectory_WithExistingDirectory_ShouldReturnListing()
    {
        // Arrange
        string tempDir = _tempHelper.CreateTempDirectory();
        string subDir = Path.Combine(tempDir, "subdir");
        Directory.CreateDirectory(subDir);

        _tempHelper.CreateTempFile(tempDir, "file1.txt", "content1");
        _tempHelper.CreateTempFile(tempDir, "file2.txt", "content2");

        // Act
        string result = _tool.ListDirectory(tempDir);

        // Assert
        Assert.IsTrue(result.Contains("Directory Listing"));
        Assert.IsTrue(result.Contains("subdir/"));
        Assert.IsTrue(result.Contains("file1.txt"));
        Assert.IsTrue(result.Contains("file2.txt"));
    }

    [TestMethod]
    public void ListDirectory_WithNonExistentDirectory_ShouldReturnError()
    {
        // Arrange
        string nonExistentPath = Path.Combine(_tempHelper.CreateTempDirectory(), "nonexistent");

        // Act
        string result = _tool.ListDirectory(nonExistentPath);

        // Assert
        Assert.IsTrue(result.StartsWith("Error: Directory not found"));
        Assert.IsTrue(result.Contains(nonExistentPath));
    }

    [TestMethod]
    public async Task IntegrationTest_FileSystemToolsViaEndToEnd()
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

            // Act - Test that file system tools are available
            JsonDocument toolsResponse = await mcpClient.ListToolsAsync(cancellationToken);

            // Assert
            Assert.IsNotNull(toolsResponse);
            Assert.IsTrue(toolsResponse.RootElement.TryGetProperty("result", out var result));
            Assert.IsTrue(result.TryGetProperty("tools", out var toolsArray));

            HashSet<string?> tools = toolsArray.EnumerateArray()
                .Select(t => t.GetProperty("name").GetString())
                .ToHashSet();

            // Verify file system tools are exposed
            Assert.IsTrue(tools.Contains("read_file"));
            Assert.IsTrue(tools.Contains("write_file"));
            Assert.IsTrue(tools.Contains("read_binary_file"));
            Assert.IsTrue(tools.Contains("write_binary_file"));
            Assert.IsTrue(tools.Contains("read_file_lines"));
            Assert.IsTrue(tools.Contains("write_file_lines"));
            Assert.IsTrue(tools.Contains("insert_file_lines"));
            Assert.IsTrue(tools.Contains("list_directory"));

            // Test calling a file system tool
            string testFilePath = _tempHelper.CreateTempFile(content: "Test content");
            JsonDocument readResponse = await mcpClient.CallToolAsync("read_file",
                new { filePath = testFilePath }, cancellationToken);

            Assert.IsNotNull(readResponse);
            Assert.IsTrue(readResponse.RootElement.TryGetProperty("result", out var readResult));
        }
        finally
        {
            await service.StopAsync(cancellationToken);
        }
    }
}
