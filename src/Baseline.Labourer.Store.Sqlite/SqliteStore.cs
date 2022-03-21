using System.Threading.Tasks;

namespace Baseline.Labourer;

/// <summary>
/// An <see cref="IStore"/> implementation that persists its data in a SQLite database.
/// </summary>
public class SqliteStore : IStore
{
    /// <inheritdoc />
    public IJobLogStore JobLogStore { get; }
    
    /// <inheritdoc />
    public IResourceLocker ResourceLocker { get; }
    
    /// <inheritdoc />
    public IStoreReader Reader { get; }
    
    /// <inheritdoc />
    public IStoreWriterTransactionManager WriterTransactionManager { get; }
    
    /// <inheritdoc />
    public ValueTask BootstrapAsync()
    {
        throw new System.NotImplementedException();
    }
}