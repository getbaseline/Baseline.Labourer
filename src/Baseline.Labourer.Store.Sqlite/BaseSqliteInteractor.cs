using Microsoft.Data.Sqlite;

namespace Baseline.Labourer;

/// <summary>
/// Defines common functionality (such as opening a database connection) that all SQLite store classes could use.
/// </summary>
public abstract class BaseSqliteInteractor
{
    private readonly string _connectionString;

    protected BaseSqliteInteractor(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Creates a new connection to the SQLite database and opens it.
    /// </summary>
    /// <returns></returns>
    protected SqliteConnection NewConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();

        return connection;
    }
}
