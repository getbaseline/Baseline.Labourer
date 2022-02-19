using Baseline.Labourer.Internal;
using Baseline.Labourer.Store.Memory;

namespace Baseline.Labourer.Tests
{
    public class TestMemoryResourceLocker : MemoryResourceLocker
    {
        public TestMemoryResourceLocker(
            MemoryBackingStore memoryBackingStore, 
            IDateTimeProvider dateTimeProvider
        ) : base(memoryBackingStore, dateTimeProvider)
        {
        }
    }
}