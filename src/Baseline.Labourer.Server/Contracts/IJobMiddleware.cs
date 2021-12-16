using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Server.JobProcessorWorker;

namespace Baseline.Labourer.Server.Contracts
{
    /// <summary>
    /// Represents what all dispatched job middlewares must implement.
    /// </summary>
    public interface IJobMiddleware
    {
        /// <summary>
        /// Called when a job is started.
        /// </summary>
        /// <param name="jobContext">The job that is being started's context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        ValueTask JobStartedAsync(JobContext jobContext, CancellationToken cancellationToken);

        /// <summary>
        /// Called when a job is completed.
        /// </summary>
        /// <param name="jobContext">The job that is completed's context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        ValueTask JobCompletedAsync(JobContext jobContext, CancellationToken cancellationToken);

        /// <summary>
        /// Called when a job fails.
        /// </summary>
        /// <param name="jobContext">The job that failed's context.</param>
        /// <param name="exception">The exception that occurred as a result of the job failing, if there is one.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        ValueTask JobFailedAsync(JobContext jobContext, Exception? exception, CancellationToken cancellationToken);
    }
}