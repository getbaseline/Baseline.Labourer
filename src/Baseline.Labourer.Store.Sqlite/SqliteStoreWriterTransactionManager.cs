namespace Baseline.Labourer;

/// <summary>
/// A store writer transaction manager that creates SQLite based transaction writers.
/// </summary>
public class SqliteStoreWriterTransactionManager : IStoreWriterTransactionManager
{
    private readonly string _connectionString;

    public SqliteStoreWriterTransactionManager(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    /// <inheritdoc />
    public ITransactionalStoreWriter BeginTransaction()
    {
        return new SqliteTransactionalStoreWriter(_connectionString);
    }
}