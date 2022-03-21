using Baseline.Labourer.Internal;

namespace Baseline.Labourer.Tests;

public class TestMemoryResourceLocker : MemoryResourceLocker
{
    public TestMemoryResourceLocker(
        MemoryStoreDataContainer memoryStoreDataContainer, 
        IDateTimeProvider dateTimeProvider
    ) : base(memoryStoreDataContainer, dateTimeProvider)
    {
    }
}