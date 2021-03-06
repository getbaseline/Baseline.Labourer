using System;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Models;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer;

/// <summary>
/// Provides a contract which transaction aware store writers must implement. Should a failure occur prior to the
/// call to <see cref="CommitAsync"/>, no changes will be performed. Where the store provider
/// supports actual transactions, these will be used during the committing process to ensure that is atomic too.
/// </summary>
public interface ITransactionalStoreWriter : IAsyncDisposable
{
    /// <summary>
    /// Commits the transaction, writing any operations to the store.
    /// </summary>
    ValueTask CommitAsync();

    /// <summary>
    /// Creates and persists a server registration in the context of the current transaction.
    /// </summary>
    /// <param name="serverInstance">The server to persist.</param>
    ValueTask CreateServerAsync(ServerInstance serverInstance);

    /// <summary>
    /// Creates a heartbeat for a server in the context of the current transaction.
    /// </summary>
    /// <param name="serverId">The server to create a heartbeat for.</param>
    ValueTask CreateServerHeartbeatAsync(string serverId);

    /// <summary>
    /// Creates and persists a worker registration in the context of the current transaction.
    /// </summary>
    /// <param name="worker">The worker to persist.</param>
    ValueTask CreateWorkerAsync(Worker worker);

    /// <summary>
    /// Saves a dispatched job to the job store in the context of the current transaction.
    /// </summary>
    /// <param name="definition">The definition object.</param>
    ValueTask CreateDispatchedJobAsync(DispatchedJobDefinition definition);

    /// <summary>
    /// Creates or updates a scheduled job in the job store in the context of the current transaction.
    /// </summary>
    /// <param name="scheduledJobDefinition">The definition of the scheduled job.</param>
    ValueTask CreateOrUpdateScheduledJobAsync(ScheduledJobDefinition scheduledJobDefinition);

    /// <summary>
    /// Deletes a scheduled job from the store in the context of the current transaction.
    /// </summary>
    /// <param name="id"></param>
    ValueTask DeleteScheduledJobAsync(string id);

    /// <summary>
    /// Creates and saves a log entry against a job specified by the job id parameter.
    /// </summary>
    /// <param name="jobId">The id of the job that should have a log entry created against it.</param>
    /// <param name="logLevel">The logging level, i.e. the severity of the log.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">An optional exception, if there was one present.</param>
    ValueTask LogEntryForJobAsync(
        string jobId,
        LogLevel logLevel,
        string message,
        Exception? exception
    );

    /// <summary>
    /// Updates the persisted retries for a job in the context of the current transaction.
    /// </summary>
    /// <param name="jobId">The job id to update the retries for.</param>
    /// <param name="retries">The retries to save against the job.</param>
    ValueTask UpdateJobRetriesAsync(string jobId, uint retries);

    /// <summary>
    /// Updates the state of a job in the context of the current transaction.
    /// </summary>
    /// <param name="jobId">The id of the job to update the state of.</param>
    /// <param name="jobStatus">The status to set against the job.</param>
    /// <param name="finishedDate">The date the job finished (if the status is complete or failed).</param>
    ValueTask UpdateJobStateAsync(string jobId, JobStatus jobStatus, DateTime? finishedDate);

    /// <summary>
    /// Updates the next run date of the scheduled job in the context of the current transaction.
    /// </summary>
    /// <param name="jobId">The id of the scheduled job that needs to be updated.</param>
    /// <param name="nextRunDate">The next run date of the job.</param>
    ValueTask UpdateScheduledJobNextRunDateAsync(string jobId, DateTime nextRunDate);

    /// <summary>
    /// Updates the last run date of the scheduled job in the context of the current transaction.
    /// </summary>
    /// <param name="jobId">The id of the scheduled job that needs to be updated.</param>
    /// <param name="lastRunDate">The last run date of the job.</param>
    ValueTask UpdateScheduledJobLastRunDateAsync(string jobId, DateTime? lastRunDate);
}
