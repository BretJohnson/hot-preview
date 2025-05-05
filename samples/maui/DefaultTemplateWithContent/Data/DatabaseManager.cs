using Microsoft.Data.Sqlite;

namespace DefaultTemplateWithContent.Data;

public class DatabaseManager
{
    private List<Repository> _repositories = new List<Repository>();
    private string _connectionString = Constants.DatabasePath;
    private SqliteConnection? _mockDataDatabaseConnection;

    public void RegisterRepository(Repository repository)
    {
        if (!_repositories.Contains(repository))
        {
            _repositories.Add(repository);
        }
    }

    public async Task<SqliteConnection> OpenConnectionAsync()
    {
        SqliteConnection? connection = null;
        try
        {
            connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
        catch(Exception e)
        {
            Console.WriteLine($"Error opening database connection: {e.Message}");
            connection?.Dispose();
            throw;
        }
    }

    public async Task SwitchToMockDataDatabaseAsync()
    {
        if (_mockDataDatabaseConnection is not null)
        {
            await ClearTablesAsync();

            _mockDataDatabaseConnection.Dispose();
            _mockDataDatabaseConnection = null;
        }

        _connectionString = $"Data Source=MockDataDatabase;Mode=Memory;Cache=Shared";
        _mockDataDatabaseConnection = new SqliteConnection(_connectionString);

        // This connection remains open for the lifetime of the database, keeping it around.
        // When all connections are closed the in-memory database goes away
        await _mockDataDatabaseConnection.OpenAsync();

        foreach (var repository in _repositories)
        {
            await repository.CreateTableAsync(_mockDataDatabaseConnection);
        }
    }

    public async Task ClearTablesAsync()
    {
        await using var connection = await OpenConnectionAsync();

        foreach (var repository in _repositories)
        {
            await repository.DropTableAsync(connection);
        }
    }

    public void SwitchToNormalDatabase()
    {
        _mockDataDatabaseConnection?.Dispose();
        _mockDataDatabaseConnection = null;

        _connectionString = Constants.DatabasePath;
    }
}
