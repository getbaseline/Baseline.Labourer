using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Server.Contracts;
using Baseline.Labourer.Server.Middleware;

namespace Baseline.Labourer.Server.JobProcessorWorker
{
    /// <summary>
    /// Runs the relevant middlewares for dispatched jobs. 
    /// </summary>
    public class JobMiddlewareRunner
    {
        private static readonly List<IJobMiddleware> SystemJobMiddlewares = new List<IJobMiddleware>
        {
            new JobUpdateProgressAndCompletionStatusMiddleware(),
            new JobFailureRetryMiddleware()
        };

        private readonly ServerContext _serverContext;

        public JobMiddlewareRunner(ServerContext serverContext)
        {
            _serverContext = serverContext;
        }

        /// <summary>
        /// Runs the relevant middleware when a job is completed. System based middlewares are ran first followed by any
        /// user provided ones.
        /// </summary>
        /// <param name="jobContext">The job that is being ran's context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async ValueTask JobCompletedAsync(JobContext jobContext, CancellationToken cancellationToken)
        {
            await ExecuteAllMiddlewaresAsync(m => m.JobCompletedAsync(jobContext, cancellationToken));
        }

        /// <summary>
        /// Runs the relevant middleware when a job is started. System based middlewares are ran first followed by any
        /// user provided ones.
        /// </summary>
        /// <param name="jobContext">The job that is being ran's context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async ValueTask JobStartedAsync(JobContext jobContext, CancellationToken cancellationToken)
        {
            await ExecuteAllMiddlewaresAsync(m => m.JobStartedAsync(jobContext, cancellationToken));
        }

        /// <summary>
        /// Runs the relevant middleware when a job fails. System based middlewares are ran first followed by any
        /// user provided ones.
        /// </summary>
        /// <param name="jobContext">The job that is being ran's context.</param>
        /// <param name="exception">An exception that may have been thrown as part of the job failure.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async ValueTask JobFailedAsync(
            JobContext jobContext,
            Exception? exception,
            CancellationToken cancellationToken
        )
        {
            await ExecuteAllMiddlewaresAsync(m => m.JobFailedAsync(jobContext, exception, cancellationToken));
        }

        private async ValueTask ExecuteAllMiddlewaresAsync(Func<IJobMiddleware, ValueTask> toExecute)
        {
            foreach (var middleware in SystemJobMiddlewares)
            {
                await toExecute(middleware);
            }

            if (_serverContext.HasAdditionalDispatchedJobMiddlewares())
            {
                foreach (var middleware in _serverContext.AdditionalDispatchedJobMiddlewares)
                {
                    await toExecute(middleware);
                }
            }
        }
    }
}