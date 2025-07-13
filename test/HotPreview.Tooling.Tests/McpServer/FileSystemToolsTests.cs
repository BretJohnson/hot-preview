using Microsoft.VisualStudio.TestTools.UnitTesting;
using HotPreview.Tooling.McpServer;
using HotPreview.Tooling.Tests.McpServer.TestHelpers;
using Microsoft.Extensions.Logging;

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
        
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
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
        var content = "Hello, World!\nThis is a test file.";
        var filePath = _tempHelper.CreateTempFile(content: content);

        // Act
        var result = _tool.ReadFile(filePath);

        // Assert
        Assert.AreEqual(content, result);
    }

    [TestMethod]
    public void ReadFile_WithNonExistentFile_ShouldReturnError()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempHelper.CreateTempDirectory(), "nonexistent.txt");

        // Act
        var result = _tool.ReadFile(nonExistentPath);

        // Assert
        Assert.IsTrue(result.StartsWith("Error: File not found"));
        Assert.IsTrue(result.Contains(nonExistentPath));
    }

    [TestMethod]
    public void WriteFile_ShouldCreateFileWithContent()
    {
        // Arrange
        var content = "Test content to write";
        var filePath = Path.Combine(_tempHelper.CreateTempDirectory(), "test.txt");

        // Act
        var result = _tool.WriteFile(filePath, content);

        // Assert
        Assert.IsTrue(result.StartsWith("Successfully wrote content"));
        Assert.IsTrue(File.Exists(filePath));
        Assert.AreEqual(content, File.ReadAllText(filePath));
    }

    [TestMethod]
    public void ReadBinaryFile_WithExistingFile_ShouldReturnBase64()
    {
        // Arrange
        var binaryData = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello" in bytes
        var filePath = Path.Combine(_tempHelper.CreateTempDirectory(), "test.bin");
        File.WriteAllBytes(filePath, binaryData);

        // Act
        var result = _tool.ReadBinaryFile(filePath);

        // Assert
        var expectedBase64 = Convert.ToBase64String(binaryData);
        Assert.AreEqual(expectedBase64, result);
    }

    [TestMethod]
    public void WriteBinaryFile_ShouldCreateFileFromBase64()
    {
        // Arrange
        var binaryData = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello" in bytes
        var base64Content = Convert.ToBase64String(binaryData);
        var filePath = Path.Combine(_tempHelper.CreateTempDirectory(), "test.bin");

        // Act
        var result = _tool.WriteBinaryFile(filePath, base64Content);

        // Assert
        Assert.IsTrue(result.StartsWith("Successfully wrote binary content"));
        Assert.IsTrue(File.Exists(filePath));
        CollectionAssert.AreEqual(binaryData, File.ReadAllBytes(filePath));
    }

    [TestMethod]
    public void ReadFileLines_WithValidRange_ShouldReturnSpecificLines()
    {
        // Arrange
        var lines = new[] { "Line 1", "Line 2", "Line 3", "Line 4", "Line 5" };
        var content = string.Join(Environment.NewLine, lines);
        var filePath = _tempHelper.CreateTempFile(content: content);

        // Act
        var result = _tool.ReadFileLines(filePath, 2, 4);

        // Assert
        var expectedContent = string.Join(Environment.NewLine, lines[1..4]);
        Assert.AreEqual(expectedContent, result);
    }

    [TestMethod]
    public void ReadFileLines_WithInvalidRange_ShouldReturnError()
    {
        // Arrange
        var filePath = _tempHelper.CreateTempFile(content: "Line 1\nLine 2");

        // Act & Assert
        var result1 = _tool.ReadFileLines(filePath, 0, 2);
        Assert.IsTrue(result1.StartsWith("Error: Start line must be at least 1"));

        var result2 = _tool.ReadFileLines(filePath, 3, 1);
        Assert.IsTrue(result2.StartsWith("Error: End line must be greater than"));
    }

    [TestMethod]
    public void WriteFileLines_ShouldReplaceSpecificLines()
    {
        // Arrange
        var initialContent = "Line 1\nLine 2\nLine 3\nLine 4";
        var filePath = _tempHelper.CreateTempFile(content: initialContent);
        var newContent = "New Line 2\nNew Line 3";

        // Act
        var result = _tool.WriteFileLines(filePath, newContent, 2);

        // Assert
        Assert.IsTrue(result.StartsWith("Successfully wrote content"));
        var fileContent = File.ReadAllText(filePath);
        var expectedContent = "Line 1\nNew Line 2\nNew Line 3\nLine 4";
        Assert.AreEqual(expectedContent, fileContent);
    }

    [TestMethod]
    public void InsertFileLines_ShouldInsertContentAtSpecificLine()
    {
        // Arrange
        var initialContent = "Line 1\nLine 2\nLine 3";
        var filePath = _tempHelper.CreateTempFile(content: initialContent);
        var insertContent = "Inserted Line";

        // Act
        var result = _tool.InsertFileLines(filePath, insertContent, 2);

        // Assert
        Assert.IsTrue(result.StartsWith("Successfully inserted content"));
        var fileContent = File.ReadAllText(filePath);
        var expectedContent = "Line 1\nInserted Line\nLine 2\nLine 3";
        Assert.AreEqual(expectedContent, fileContent);
    }

    [TestMethod]
    public void ListDirectory_WithExistingDirectory_ShouldReturnListing()
    {
        // Arrange
        var tempDir = _tempHelper.CreateTempDirectory();
        var subDir = Path.Combine(tempDir, "subdir");
        Directory.CreateDirectory(subDir);
        
        _tempHelper.CreateTempFile(tempDir, "file1.txt", "content1");
        _tempHelper.CreateTempFile(tempDir, "file2.txt", "content2");

        // Act
        var result = _tool.ListDirectory(tempDir);

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
        var nonExistentPath = Path.Combine(_tempHelper.CreateTempDirectory(), "nonexistent");

        // Act
        var result = _tool.ListDirectory(nonExistentPath);

        // Assert
        Assert.IsTrue(result.StartsWith("Error: Directory not found"));
        Assert.IsTrue(result.Contains(nonExistentPath));
    }

    [TestMethod]
    public async Task IntegrationTest_FileSystemToolsViaEndToEnd()
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

            // Act - Test that file system tools are available
            var toolsResponse = await mcpClient.ListToolsAsync(cancellationToken);

            // Assert
            Assert.IsNotNull(toolsResponse);
            Assert.IsTrue(toolsResponse.RootElement.TryGetProperty("result", out var result));
            Assert.IsTrue(result.TryGetProperty("tools", out var toolsArray));

            var tools = toolsArray.EnumerateArray()
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
            var testFilePath = _tempHelper.CreateTempFile(content: "Test content");
            var readResponse = await mcpClient.CallToolAsync("read_file", 
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