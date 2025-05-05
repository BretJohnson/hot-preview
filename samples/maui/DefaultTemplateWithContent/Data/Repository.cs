using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace DefaultTemplateWithContent.Data;

/// <summary>
/// Base class for managing data in the database.
/// </summary>
public abstract class Repository
{
    private readonly DatabaseManager _databaseManager;
    private bool _hasBeenInitialized = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagRepository"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public Repository(DatabaseManager databaseManager, ILogger logger)
    {
        _databaseManager = databaseManager;
        Logger = logger;

        _databaseManager.RegisterRepository(this);
    }

    protected ILogger Logger { get; }

    private async Task Init()
    {
        if (_hasBeenInitialized)
            return;

        using var connection = await _databaseManager.OpenConnectionAsync();
        await CreateTableAsync(connection);

        _hasBeenInitialized = true;
    }

    protected async Task<SqliteConnection> OpenConnectionAsync()
    {
        await Init();
        return await _databaseManager.OpenConnectionAsync();
    }

    public abstract Task CreateTableAsync(SqliteConnection connection);

    public abstract Task DropTableAsync(SqliteConnection connection);
}
