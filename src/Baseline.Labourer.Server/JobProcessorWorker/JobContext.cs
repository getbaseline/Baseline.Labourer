using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Contracts;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Models;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.JobProcessorWorker
{
    /// <summary>
    /// JobContext provides context and dependencies around a job that is running/is to be ran.
    /// </summary>
    public class JobContext
    {
        /// <summary>
        /// Gets or sets the id of the message that this 
        /// </summary>
        public string OriginalMessageId { get; set; }

        /// <summary>
        /// Gets or sets the context of the worker that the job is running in.
        /// </summary>
        public WorkerContext WorkerContext { get; set; }

        /// <summary>
        /// Gets or sets the definition of the job that is being ran.
        /// </summary>
        public DispatchedJobDefinition JobDefinition { get; set; }

        /// <summary>
        /// Alias to the server context's <see cref="IStoreWriterTransactionManager.BeginTransaction"/> method.
        /// </summary>
        public ITransactionalStoreWriter BeginTransaction()
        {
            return WorkerContext.ServerContext.StoreWriterTransactionManager.BeginTransaction();
        }

        /// <summary>
        /// Updates the job's state.
        /// </summary>
        /// <param name="writer">A transactionized store writer to use.</param>
        /// <param name="status">The new status of the job.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task UpdateJobStateAsync(
            ITransactionalStoreWriter writer,
            JobStatus status,
            CancellationToken cancellationToken
        )
        {
            WorkerContext.ServerContext.JobLogStore.LogEntryForJob(
                JobDefinition.Id,
                LogLevel.Information,
                $"Job status changed from {JobDefinition.Status} to {status}.",
                null
            );

            JobDefinition.Status = status;

            await writer.UpdateJobStateAsync(
                JobDefinition.Id,
                status,
                status == JobStatus.Complete || status == JobStatus.FailedExceededMaximumRetries ? (DateTime?) DateTime.UtcNow : null,
                cancellationToken
            );
        }

        /// <summary>
        /// Increments the number of retries the job has had.
        /// </summary>
        /// <param name="writer">A transactionized store writer to use.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task IncrementJobRetriesAsync(ITransactionalStoreWriter writer, CancellationToken cancellationToken)
        {
            JobDefinition.Retries += 1;

            await writer.UpdateJobRetriesAsync(
                JobDefinition.Id,
                JobDefinition.Retries,
                cancellationToken
            );
        }

        /// <summary>
        /// Re-queues a job.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task RequeueJobAsync(CancellationToken cancellationToken)
        {
            await WorkerContext.ServerContext.Queue.EnqueueAsync(JobDefinition, cancellationToken);
        }

        /// <summary>
        /// Removes the job from the queue to ensure it is not retried.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task RemoveMessageFromQueueAsync(CancellationToken cancellationToken)
        {
            await WorkerContext.ServerContext.Queue.DeleteMessageAsync(OriginalMessageId, cancellationToken);
        }
    }
}