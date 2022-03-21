namespace Baseline.Labourer;

/// <summary>
/// A store writer transaction manager that creates SQLite based transaction writers.
/// </summary>
public class SqliteStoreWriterTransactionManager : IStoreWriterTransactionManager
{
    /// <inheritdoc />
    public ITransactionalStoreWriter BeginTransaction()
    {
        throw new System.NotImplementedException();
    }
}