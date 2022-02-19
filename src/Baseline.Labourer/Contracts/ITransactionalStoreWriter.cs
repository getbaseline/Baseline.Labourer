using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;

namespace Baseline.Labourer
{
    /// <summary>
    /// Provides a contract which transaction aware store writers must implement. Should a failure occur prior to the
    /// call to <see cref="CommitAsync(CancellationToken)"/>, no changes will be performed. Where the store provider
    /// supports actual transactions, these will be used during the committing process to ensure that is atomic too.
    /// </summary>
    public interface ITransactionalStoreWriter : IAsyncDisposable
    {
        /// <summary>
        /// Commits the transaction, writing any operations to the store.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        ValueTask CommitAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Creates and persists a server registration in the context of the current transaction.
        /// </summary>
        /// <param name="serverInstance">The server to persist.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        ValueTask CreateServerAsync(ServerInstance serverInstance, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a heartbeat for a server in the context of the current transaction.
        /// </summary>
        /// <param name="serverId">The server to create a heartbeat for.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        ValueTask CreateServerHeartbeatAsync(string serverId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates and persists a worker registration in the context of the current transaction.
        /// </summary>
        /// <param name="worker">The worker to persist.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        ValueTask CreateWorkerAsync(Worker worker, CancellationToken cancellationToken);

        /// <summary>
        /// Saves a dispatched job to the job store in the context of the current transaction.
        /// </summary>
        /// <param name="definition">The definition object.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        ValueTask CreateDispatchedJobAsync(
            DispatchedJobDefinition definition,
            CancellationToken cancellationToken
        );

        /// <summary>
        /// Creates or updates a scheduled job in the job store in the context of the current transaction.
        /// </summary>
        /// <param name="scheduledJobDefinition">The definition of the scheduled job.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        ValueTask CreateOrUpdateScheduledJobAsync(
            ScheduledJobDefinition scheduledJobDefinition, 
            CancellationToken cancellationToken
        );

        /// <summary>
        /// Deletes a scheduled job from the store in the context of the current transaction.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask DeleteScheduledJobAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the persisted retries for a job in the context of the current transaction.
        /// </summary>
        /// <param name="jobId">The job id to update the retries for.</param>
        /// <param name="retries">The retries to save against the job.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        ValueTask UpdateJobRetriesAsync(string jobId, uint retries, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the state of a job in the context of the current transaction.
        /// </summary>
        /// <param name="jobId">The id of the job to update the state of.</param>
        /// <param name="jobStatus">The status to set against the job.</param>
        /// <param name="finishedDate">The date the job finished (if the status is complete or failed).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        ValueTask UpdateJobStateAsync(
            string jobId,
            JobStatus jobStatus,
            DateTime? finishedDate,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Updates the next run date of the scheduled job in the context of the current transaction.
        /// </summary>
        /// <param name="jobId">The id of the scheduled job that needs to be updated.</param>
        /// <param name="nextRunDate">The next run date of the job.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        ValueTask UpdateScheduledJobNextRunDateAsync(
            string jobId, 
            DateTime nextRunDate, 
            CancellationToken cancellationToken
        );

        /// <summary>
        /// Updates the last run date of the scheduled job in the context of the current transaction.
        /// </summary>
        /// <param name="jobId">The id of the scheduled job that needs to be updated.</param>
        /// <param name="lastRunDate">The last run date of the job.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        ValueTask UpdateScheduledJobLastRunDateAsync(
            string jobId, 
            DateTime? lastRunDate, 
            CancellationToken cancellationToken
        );
    }
}