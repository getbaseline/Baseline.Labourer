namespace Baseline.Labourer.Store.Memory;

/// <summary>
/// Memory store based transaction manager that handles the creation of a transaction.
/// </summary>
public class MemoryStoreWriterTransactionManager : IStoreWriterTransactionManager
{
    private readonly MemoryStoreDataContainer _memoryStoreDataContainer;

    public MemoryStoreWriterTransactionManager(MemoryStoreDataContainer memoryStoreDataContainer)
    {
        _memoryStoreDataContainer = memoryStoreDataContainer;
    }

    /// <inheritdoc />
    public ITransactionalStoreWriter BeginTransaction()
    {
        return new MemoryTransactionalStoreWriter(_memoryStoreDataContainer);
    }
}
