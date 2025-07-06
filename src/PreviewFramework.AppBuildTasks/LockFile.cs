
namespace PreviewFramework.AppBuildTasks
{
    /// <summary>
    /// Represents an exclusive file lock that automatically deletes the file when disposed.
    /// </summary>
    internal sealed class LockFile : IDisposable
    {
        private readonly string _filePath;
        private readonly FileStream _fileStream;
        private bool _disposed;

        private LockFile(string filePath, FileStream fileStream)
        {
            _filePath = filePath;
            _fileStream = fileStream;
        }

        /// <summary>
        /// Creates an exclusive lock file. Throws IOException if another process already has the file locked.
        /// </summary>
        public static LockFile Create(string filePath, string content)
        {
            var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            try
            {
                using var writer = new StreamWriter(fileStream, System.Text.Encoding.UTF8, bufferSize: 1024, leaveOpen: true);
                writer.WriteLine(content);
                writer.Flush();
                return new LockFile(filePath, fileStream);
            }
            catch
            {
                fileStream.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _fileStream?.Dispose();

                // Delete the file after closing the stream
                try
                {
                    if (File.Exists(_filePath))
                    {
                        File.Delete(_filePath);
                    }
                }
                catch
                {
                    // Ignore errors when cleaning up lock file
                }

                _disposed = true;
            }
        }
    }
}
