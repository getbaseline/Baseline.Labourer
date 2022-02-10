using Baseline.Labourer.Contracts;

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
        IStoreReader StoreReader { get; }
        
        /// <summary>
        /// Gets the store writer transaction manager.
        /// </summary>
        IStoreWriterTransactionManager StoreWriterTransactionManager { get; }
    }
}