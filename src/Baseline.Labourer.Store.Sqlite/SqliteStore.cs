using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Contracts;
using Baseline.Labourer.Shared.Sqlite;

namespace Baseline.Labourer.Store.Sqlite;

/// <summary>
/// An <see cref="IStore"/> implementation that persists its data in a SQLite database.
/// </summary>
public class SqliteStore : BaseSqliteBootstrapper<SqliteStore>, IStore
{
    /// <inheritdoc />
    public IResourceLocker ResourceLocker { get; }

    /// <inheritdoc />
    public IStoreReader Reader { get; }

    /// <inheritdoc />
    public IStoreWriterTransactionManager WriterTransactionManager { get; }

    public SqliteStore(string connectionString) : this(new DateTimeProvider(), connectionString) { }

    public SqliteStore(IDateTimeProvider dateTimeProvider, string connectionString)
        : base(connectionString)
    {
        ResourceLocker = new SqliteResourceLocker(dateTimeProvider, connectionString);
        Reader = new SqliteReader(connectionString);
        WriterTransactionManager = new SqliteStoreWriterTransactionManager(
            dateTimeProvider,
            connectionString
        );
    }
}
