namespace Baseline.Labourer.Contracts
{
    /// <summary>
    /// Allows a write transaction to be created. This ensures writes only occur when every operation completes successfully.
    /// </summary>
    public interface IStoreWriterTransactionManager
    {
        /// <summary>
        /// Begins a transaction, returning a transactional store writer that can be used to chain writes to only run once committed.
        /// </summary>
        ITransactionalStoreWriter BeginTransaction();
    }
}