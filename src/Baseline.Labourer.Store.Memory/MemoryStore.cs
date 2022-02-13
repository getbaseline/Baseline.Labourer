using System.Threading.Tasks;

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
        public IStoreReader Reader { get; }
        
        /// <inheritdoc />
        public IStoreWriterTransactionManager WriterTransactionManager { get; }

        public MemoryStore() : this(new MemoryBackingStore())
        {
        }

        public MemoryStore(MemoryBackingStore memoryBackingStore)
        {
            JobLogStore = new MemoryJobLogStore(memoryBackingStore);
            ResourceLocker = new MemoryResourceLocker(memoryBackingStore);
            Reader = new MemoryStoreReader(memoryBackingStore);
            WriterTransactionManager = new MemoryStoreWriterTransactionManager(memoryBackingStore);
        }

        /// <inheritdoc />
        public ValueTask BootstrapAsync()
        {
            return new ValueTask();
        }
    }
}