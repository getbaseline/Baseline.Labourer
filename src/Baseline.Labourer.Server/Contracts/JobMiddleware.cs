using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Server.JobProcessorWorker;

namespace Baseline.Labourer.Server.Contracts
{
    /// <summary>
    /// Helper base class for dispatched job middlewares. Allows relevant implementations to only implement middleware
    /// functions that they care about.
    /// </summary>
    public abstract class JobMiddleware : IJobMiddleware
    {
        /// <inheritdoc />
        public virtual ValueTask JobStartedAsync(JobContext jobContext, CancellationToken cancellationToken)
        {
            return new ValueTask();
        }

        /// <inheritdoc />
        public virtual ValueTask JobCompletedAsync(JobContext jobContext, CancellationToken cancellationToken)
        {
            return new ValueTask();
        }

        /// <inheritdoc />
        public virtual ValueTask JobFailedAsync(
            JobContext jobContext, 
            Exception? exception, 
            CancellationToken cancellationToken
        )
        {
            return new ValueTask();
        }
    }
}