using System.Threading.Tasks;
using Baseline.Labourer.Internal;

namespace Baseline.Labourer.Tests;

public class TestMemoryStore : IStore
{
    public bool Bootstrapped { get; private set; }
    public IResourceLocker ResourceLocker { get; }
    public IStoreReader Reader { get; }
    public IStoreWriterTransactionManager WriterTransactionManager { get; }

    public TestMemoryStore(
        TestMemoryStoreDataContainer memoryStoreDataContainer,
        IDateTimeProvider dateTimeProvider
    )
    {
        ResourceLocker = new TestMemoryResourceLocker(memoryStoreDataContainer, dateTimeProvider);
        Reader = new MemoryStoreReader(memoryStoreDataContainer);
        WriterTransactionManager = new MemoryStoreWriterTransactionManager(
            memoryStoreDataContainer
        );
    }

    public ValueTask BootstrapAsync()
    {
        Bootstrapped = true;
        return new ValueTask();
    }
}
