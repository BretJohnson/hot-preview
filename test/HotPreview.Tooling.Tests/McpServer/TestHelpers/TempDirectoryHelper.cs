namespace HotPreview.Tooling.Tests.McpServer.TestHelpers;

public class TempDirectoryHelper : IDisposable
{
    private readonly List<string> _tempDirectories = new();
    private bool _disposed;

    public string CreateTempDirectory(string? prefix = null)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"{prefix ?? "mcptest"}_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        _tempDirectories.Add(tempDir);
        return tempDir;
    }

    public string CreateTempFile(string? directory = null, string? fileName = null, string? content = null)
    {
        directory ??= CreateTempDirectory();
        fileName ??= $"test_{Guid.NewGuid():N}.txt";

        var filePath = Path.Combine(directory, fileName);
        if (content != null)
        {
            File.WriteAllText(filePath, content);
        }
        else
        {
            File.Create(filePath).Dispose();
        }

        return filePath;
    }

    public void Dispose()
    {
        if (_disposed) return;

        foreach (var directory in _tempDirectories)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete temp directory {directory}: {ex.Message}");
            }
        }

        _tempDirectories.Clear();
        _disposed = true;
    }
}
