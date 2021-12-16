using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Server.Contracts;
using Baseline.Labourer.Server.JobProcessorWorker;

namespace Baseline.Labourer.Server.Middleware
{
    /// <summary>
    /// Middleware for updating the status of the relevant job based on events that occur.
    /// </summary>
    public class JobUpdateProgressAndCompletionStatusMiddleware : JobMiddleware
    {
        /// <summary>
        /// Updates the job's status to mark it as completed.
        /// </summary>
        /// <param name="jobContext">The job's context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public override async ValueTask JobCompletedAsync(JobContext jobContext, CancellationToken cancellationToken)
        {
            await using var writer = jobContext.BeginTransaction();
            await jobContext.UpdateJobStateAsync(writer, JobStatus.Complete, cancellationToken);
            await writer.CommitAsync(cancellationToken);
        }
        
        /// <summary>
        /// Updates the job's status to mark it as in progress.
        /// </summary>
        /// <param name="jobContext">The job's context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public override async ValueTask JobStartedAsync(JobContext jobContext, CancellationToken cancellationToken)
        {
            await using var writer = jobContext.BeginTransaction();
            await jobContext.UpdateJobStateAsync(writer, JobStatus.InProgress, cancellationToken);
            await writer.CommitAsync(cancellationToken);
        }

        /// <summary>
        /// Updates the job's status to mark it as failed and that it has exceeded all retries.
        /// </summary>
        /// <param name="jobContext">The jobs context.</param>
        /// <param name="exception">The exception (if there was one).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns></returns>
        public override async ValueTask JobFailedAndExceededRetriesAsync(
            JobContext jobContext, 
            Exception? exception,
            CancellationToken cancellationToken
        )
        {
            await using var writer = jobContext.BeginTransaction();
            await jobContext.UpdateJobStateAsync(writer, JobStatus.FailedExceededMaximumRetries, cancellationToken);
            await writer.CommitAsync(cancellationToken);
        }
    }
}