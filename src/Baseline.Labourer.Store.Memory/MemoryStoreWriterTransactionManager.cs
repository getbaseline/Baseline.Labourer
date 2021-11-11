using Baseline.Labourer.Contracts;

namespace Baseline.Labourer.Store.Memory
{
    /// <summary>
    /// Memory store based transaction manager that handles the creation of a transaction.
    /// </summary>
    public class MemoryStoreWriterTransactionManager : IStoreWriterTransactionManager
    {
        private readonly MemoryStore _memoryStore;

        public MemoryStoreWriterTransactionManager(MemoryStore memoryStore)
        {
            _memoryStore = memoryStore;
        }

        /// <inheritdoc />
        public ITransactionalStoreWriter BeginTransaction()
        {
            return new MemoryTransactionalStoreWriter(_memoryStore);
        }
    }
}