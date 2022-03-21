namespace Baseline.Labourer;

/// <summary>
/// NoOpStoreWriterTransactionManager is an <see cref="IStoreWriterTransactionManager"/> that does literally nothing.
/// </summary>
public class NoOpStoreWriterTransactionManager : IStoreWriterTransactionManager
{
    /// <inheritdoc />
    public ITransactionalStoreWriter BeginTransaction()
    {
        return new NoOpTransactionalStoreWriter();
    }
}