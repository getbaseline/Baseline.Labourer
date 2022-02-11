using System.Threading.Tasks;
using Baseline.Labourer.Contracts;
using Baseline.Labourer.Internal.Contracts;
using Baseline.Labourer.Store.Memory;

namespace Baseline.Labourer.Tests
{
    public class TestMemoryStore : IStore
    {
        public bool Bootstrapped { get; private set; }
        public IJobLogStore JobLogStore { get; }
        public IResourceLocker ResourceLocker { get; }
        public IStoreReader Reader { get; }
        public IStoreWriterTransactionManager WriterTransactionManager { get; }
        
        public TestMemoryStore(TestMemoryBackingStore memoryBackingStore, IDateTimeProvider dateTimeProvider)
        {
            JobLogStore = new MemoryJobLogStore(memoryBackingStore);
            ResourceLocker = new TestMemoryResourceLocker(memoryBackingStore, dateTimeProvider);
            Reader = new MemoryStoreReader(memoryBackingStore);
            WriterTransactionManager = new MemoryStoreWriterTransactionManager(memoryBackingStore);
        }
        
        public ValueTask BootstrapAsync()
        {
            Bootstrapped = true;
            return new ValueTask();
        }
    }
}