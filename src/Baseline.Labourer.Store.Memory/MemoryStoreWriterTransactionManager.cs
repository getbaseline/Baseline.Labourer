namespace Baseline.Labourer.Store.Memory
{
    /// <summary>
    /// Memory store based transaction manager that handles the creation of a transaction.
    /// </summary>
    public class MemoryStoreWriterTransactionManager : IStoreWriterTransactionManager
    {
        private readonly MemoryBackingStore _memoryBackingStore;

        public MemoryStoreWriterTransactionManager(MemoryBackingStore memoryBackingStore)
        {
            _memoryBackingStore = memoryBackingStore;
        }

        /// <inheritdoc />
        public ITransactionalStoreWriter BeginTransaction()
        {
            return new MemoryTransactionalStoreWriter(_memoryBackingStore);
        }
    }
}