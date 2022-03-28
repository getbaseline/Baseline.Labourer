using Baseline.Labourer.Internal;

namespace Baseline.Labourer;

/// <summary>
/// An <see cref="IStore"/> implementation that persists its data in a SQLite database.
/// </summary>
public class SqliteStore : BaseSqliteBootstrapper<SqliteStore>, IStore
{
    /// <inheritdoc />
    public IJobLogStore JobLogStore { get; }

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
        JobLogStore = new SqliteJobLogStore(connectionString);
        ResourceLocker = new SqliteResourceLocker(dateTimeProvider, connectionString);
        Reader = new SqliteReader(connectionString);
        WriterTransactionManager = new SqliteStoreWriterTransactionManager(
            dateTimeProvider,
            connectionString
        );
    }
}
