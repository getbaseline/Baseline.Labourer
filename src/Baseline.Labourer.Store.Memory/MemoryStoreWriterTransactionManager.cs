using Baseline.Labourer.Contracts;

namespace Baseline.Labourer.Store.Memory;

public class MemoryStoreWriterTransactionManager : IStoreWriterTransactionManager
{
    private readonly MemoryStore _memoryStore;

    public MemoryStoreWriterTransactionManager(MemoryStore memoryStore)
    {
        _memoryStore = memoryStore;
    }

    public ITransactionalStoreWriter BeginTransaction()
    {
        return new MemoryTransactionalStoreWriter(_memoryStore);
    }
}
