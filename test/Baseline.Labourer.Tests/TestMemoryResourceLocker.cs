using Baseline.Labourer.Internal;

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