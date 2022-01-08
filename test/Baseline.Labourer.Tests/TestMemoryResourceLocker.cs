using Baseline.Labourer.Internal.Contracts;
using Baseline.Labourer.Store.Memory;

namespace Baseline.Labourer.Tests
{
    public class TestMemoryResourceLocker : MemoryResourceLocker
    {
        public TestMemoryResourceLocker(
            MemoryStore memoryStore, 
            IDateTimeProvider dateTimeProvider
        ) : base(memoryStore, dateTimeProvider)
        {
        }
    }
}