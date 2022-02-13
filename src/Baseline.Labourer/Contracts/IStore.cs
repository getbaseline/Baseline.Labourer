using System.Threading.Tasks;

namespace Baseline.Labourer
{
    /// <summary>
    /// Provides an interface that all stores must implement to provide easy access to their relevant store contract
    /// implementations.
    /// </summary>
    public interface IStore
    {
        /// <summary>
        /// Gets the job log store.
        /// </summary>
        IJobLogStore JobLogStore { get; }
        
        /// <summary>
        /// Gets the resource locker.
        /// </summary>
        IResourceLocker ResourceLocker { get; }
        
        /// <summary>
        /// Gets the store reader.
        /// </summary>
        IStoreReader Reader { get; }
        
        /// <summary>
        /// Gets the store writer transaction manager.
        /// </summary>
        IStoreWriterTransactionManager WriterTransactionManager { get; }

        /// <summary>
        /// Bootstraps the current store (i.e. runs database migrations if using a database backed store).
        /// </summary>
        ValueTask BootstrapAsync();
    }
}