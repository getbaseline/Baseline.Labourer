using Baseline.Labourer.Contracts;

namespace Baseline.Labourer.Store.Memory
{
    /// <summary>
    /// Provides a memory based <see cref="IStore"/> with all relevant store contract implementations contained within.
    /// </summary>
    public class MemoryStore : IStore
    {
        /// <inheritdoc />
        public IJobLogStore JobLogStore { get; }
        
        /// <inheritdoc />
        public IResourceLocker ResourceLocker { get; }
        
        /// <inheritdoc />
        public IStoreReader StoreReader { get; }
        
        /// <inheritdoc />
        public IStoreWriterTransactionManager StoreWriterTransactionManager { get; }

        public MemoryStore() : this(new MemoryBackingStore())
        {
        }

        public MemoryStore(MemoryBackingStore memoryBackingStore)
        {
            JobLogStore = new MemoryJobLogStore(memoryBackingStore);
            ResourceLocker = new MemoryResourceLocker(memoryBackingStore);
            StoreReader = new MemoryStoreReader(memoryBackingStore);
            StoreWriterTransactionManager = new MemoryStoreWriterTransactionManager(memoryBackingStore);
        }
    }
}