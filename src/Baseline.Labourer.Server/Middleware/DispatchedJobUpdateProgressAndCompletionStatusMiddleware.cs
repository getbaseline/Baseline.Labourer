using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Server.Contracts;
using Baseline.Labourer.Server.JobProcessorWorker;

namespace Baseline.Labourer.Server.Middleware
{
    public class DispatchedJobUpdateProgressAndCompletionStatusMiddleware : DispatchedJobMiddleware
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
    }
}