using Baseline.Labourer.Contracts;
using Baseline.Labourer.Internal.Contracts;
using Baseline.Labourer.Store.Memory;

namespace Baseline.Labourer.Tests
{
    public class TestMemoryStore : IStore
    {
        public IJobLogStore JobLogStore { get; }
        public IResourceLocker ResourceLocker { get; }
        public IStoreReader StoreReader { get; }
        public IStoreWriterTransactionManager StoreWriterTransactionManager { get; }

        public TestMemoryStore(TestMemoryBackingStore memoryBackingStore, IDateTimeProvider dateTimeProvider)
        {
            JobLogStore = new MemoryJobLogStore(memoryBackingStore);
            ResourceLocker = new TestMemoryResourceLocker(memoryBackingStore, dateTimeProvider);
            StoreReader = new MemoryStoreReader(memoryBackingStore);
            StoreWriterTransactionManager = new MemoryStoreWriterTransactionManager(memoryBackingStore);
        }
    }
}