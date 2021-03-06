using System.Threading.Tasks;

namespace Baseline.Labourer.Store.Memory;

/// <summary>
/// Provides a memory based <see cref="IStore"/> with all relevant store contract implementations contained within.
/// </summary>
public class MemoryStore : IStore
{
    /// <inheritdoc />
    public IResourceLocker ResourceLocker { get; }

    /// <inheritdoc />
    public IStoreReader Reader { get; }

    /// <inheritdoc />
    public IStoreWriterTransactionManager WriterTransactionManager { get; }

    public MemoryStore() : this(new MemoryStoreDataContainer()) { }

    public MemoryStore(MemoryStoreDataContainer memoryStoreDataContainer)
    {
        ResourceLocker = new MemoryResourceLocker(memoryStoreDataContainer);
        Reader = new MemoryStoreReader(memoryStoreDataContainer);
        WriterTransactionManager = new MemoryStoreWriterTransactionManager(
            memoryStoreDataContainer
        );
    }

    /// <inheritdoc />
    public ValueTask BootstrapAsync()
    {
        return new ValueTask();
    }
}
