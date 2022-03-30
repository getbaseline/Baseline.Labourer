using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server;

/// <summary>
/// JobContext provides context and dependencies around a job that is running/is to be ran.
/// </summary>
public class JobContext
{
    /// <summary>
    /// Gets or sets the id of the message that this
    /// </summary>
    public string OriginalMessageId { get; }

    /// <summary>
    /// Gets or sets the context of the worker that the job is running in.
    /// </summary>
    public WorkerContext WorkerContext { get; }

    /// <summary>
    /// Gets or sets the definition of the job that is being ran.
    /// </summary>
    public DispatchedJobDefinition JobDefinition { get; }

    public JobContext(
        string originalMessageId,
        WorkerContext workerContext,
        DispatchedJobDefinition jobDefinition
    )
    {
        OriginalMessageId = originalMessageId;
        WorkerContext = workerContext;
        JobDefinition = jobDefinition;
    }

    /// <summary>
    /// Alias to the server context's <see cref="IStoreWriterTransactionManager.BeginTransaction"/> method.
    /// </summary>
    public ITransactionalStoreWriter BeginTransaction()
    {
        return WorkerContext.ServerContext.Store.WriterTransactionManager.BeginTransaction();
    }

    /// <summary>
    /// Gets the type of the job that this job context relates to.
    /// </summary>
    public Type JobType => Type.GetType(JobDefinition.Type)!;

    /// <summary>
    /// Updates the job's state.
    /// </summary>
    /// <param name="writer">A transactionized store writer to use.</param>
    /// <param name="status">The new status of the job.</param>
    public async Task UpdateJobStateAsync(ITransactionalStoreWriter writer, JobStatus status)
    {
        // TODO: this will cause a lock. Move this functionality into the transaction manager.
        await writer.LogEntryForJobAsync(
            JobDefinition.Id,
            LogLevel.Information,
            $"Job status changed from {JobDefinition.Status} to {status}.",
            null
        );

        JobDefinition.Status = status;

        await writer.UpdateJobStateAsync(
            JobDefinition.Id,
            status,
            status is JobStatus.Complete or JobStatus.FailedExceededMaximumRetries
              ? (DateTime?)DateTime.UtcNow
              : null
        );
    }

    /// <summary>
    /// Increments the number of retries the job has had.
    /// </summary>
    /// <param name="writer">A transactionized store writer to use.</param>
    public async Task IncrementJobRetriesAsync(ITransactionalStoreWriter writer)
    {
        JobDefinition.Retries += 1;

        await writer.UpdateJobRetriesAsync(JobDefinition.Id, JobDefinition.Retries);
    }

    /// <summary>
    /// Removes the job from the queue to ensure it is not retried.
    /// </summary>
    public async Task RemoveMessageFromQueueAsync()
    {
        await WorkerContext.ServerContext.Queue.DeleteMessageAsync(OriginalMessageId);
    }

    /// <summary>
    /// Re-queues a job.
    /// </summary>
    public async Task RequeueJobAsync()
    {
        await WorkerContext.ServerContext.Queue.EnqueueAsync(JobDefinition, RetryDelayForJob());
    }

    /// <summary>
    /// Gets the retry count for the current job (if one has been configured) or the default retry count.
    /// </summary>
    public uint RetryCountForJob()
    {
        var retryCount = WorkerContext.ServerContext.DefaultRetryConfiguration.Retries;

        if (WorkerContext.ServerContext.JobRetryConfigurations.ContainsKey(JobType))
        {
            retryCount = WorkerContext.ServerContext.JobRetryConfigurations[JobType].Retries;
        }

        return retryCount;
    }

    /// <summary>
    /// Locks the current job contained within this job context for a specified period of time or until the
    /// <see cref="IAsyncDisposable.DisposeAsync"/> function on the returned disposable is called.
    /// </summary>
    public async Task<IAsyncDisposable> AcquireJobLockAsync()
    {
        return await WorkerContext.ServerContext.Store.ResourceLocker.LockResourceAsync(
            JobDefinition.Id,
            TimeSpan.FromSeconds(59)
        );
    }

    /// <summary>
    /// Gets the retry delay for the current job (if one has been configured) or the default retry delay.
    /// </summary>
    private TimeSpan RetryDelayForJob()
    {
        var retryDelays = WorkerContext.ServerContext.DefaultRetryConfiguration.Delays;

        if (WorkerContext.ServerContext.JobRetryConfigurations.ContainsKey(JobType))
        {
            retryDelays = WorkerContext.ServerContext.JobRetryConfigurations[JobType].Delays;
        }

        return retryDelays.ElementAt((int)JobDefinition.Retries - 1);
    }
}
