using Microsoft.Data.Sqlite;

namespace DefaultTemplateWithContent.Data;

public class DatabaseManager
{
    private List<Repository> _repositories = new List<Repository>();
    private string _connectionString = Constants.DatabasePath;
    private SqliteConnection? _testDatabaseConnection;

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

    public async Task SwitchToTestDatabase()
    {
        if (_testDatabaseConnection is not null)
        {
            await ClearTablesAsync();

            _testDatabaseConnection.Dispose();
            _testDatabaseConnection = null;
        }

        _connectionString = $"Data Source=TestDatabase;Mode=Memory;Cache=Shared";
        _testDatabaseConnection = new SqliteConnection(_connectionString);

        foreach (var repository in _repositories)
        {
            await repository.CreateTableAsync(_testDatabaseConnection);
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
        _testDatabaseConnection?.Dispose();
        _testDatabaseConnection = null;

        _connectionString = Constants.DatabasePath;
    }
}
