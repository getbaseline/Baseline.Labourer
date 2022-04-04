using Baseline.Labourer.Internal.Contracts;

namespace Baseline.Labourer.Store.Sqlite;

/// <summary>
/// A store writer transaction manager that creates SQLite based transaction writers.
/// </summary>
public class SqliteStoreWriterTransactionManager : IStoreWriterTransactionManager
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly string _connectionString;

    public SqliteStoreWriterTransactionManager(
        IDateTimeProvider dateTimeProvider,
        string connectionString
    )
    {
        _dateTimeProvider = dateTimeProvider;
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public ITransactionalStoreWriter BeginTransaction()
    {
        return new SqliteTransactionalStoreWriter(_dateTimeProvider, _connectionString);
    }
}
