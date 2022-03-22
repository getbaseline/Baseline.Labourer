using System.Threading.Tasks;

namespace Baseline.Labourer;

/// <summary>
/// NoOpStore is an <see cref="IStore"/> implementation that does literally nothing. Useful for testing if you
/// don't want jobs dispatched and/or potentially ran.
/// </summary>
public class NoOpStore : IStore
{
    /// <inheritdoc />
    public IJobLogStore JobLogStore { get; } = new NoOpJobLogStore();

    /// <inheritdoc />
    public IResourceLocker ResourceLocker { get; } = new NoOpResourceLocker();

    /// <inheritdoc />
    public IStoreReader Reader { get; } = new NoOpReader();

    /// <inheritdoc />
    public IStoreWriterTransactionManager WriterTransactionManager { get; } =
        new NoOpStoreWriterTransactionManager();

    /// <inheritdoc />
    public ValueTask BootstrapAsync()
    {
        return new ValueTask();
    }
}
