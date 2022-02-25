using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.Internal
{
    /// <summary>
    /// Runs the relevant middlewares for dispatched jobs. 
    /// </summary>
    internal class JobMiddlewareRunner
    {
        private static readonly List<IJobMiddleware> SystemJobMiddlewares = new List<IJobMiddleware>
        {
            new JobUpdateProgressAndCompletionStatusMiddleware(),
            new JobFailureRetryMiddleware()
        };

        private readonly ServerContext _serverContext;
        private readonly ILogger<JobMiddlewareRunner> _logger;

        public JobMiddlewareRunner(ServerContext serverContext)
        {
            _serverContext = serverContext;
            _logger = serverContext.LoggerFactory.CreateLogger<JobMiddlewareRunner>();
        }

        /// <summary>
        /// Runs the relevant middleware when a job is completed. System based middlewares are ran first followed by any
        /// user provided ones.
        /// </summary>
        /// <param name="jobContext">The job that is being ran's context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async ValueTask JobCompletedAsync(JobContext jobContext, CancellationToken cancellationToken)
        {
            await ExecuteAllMiddlewaresAsync(
                async m =>
                {
                    await m.JobCompletedAsync(jobContext, cancellationToken);
                    return MiddlewareContinuation.Continue;
                },
                jobContext
            );
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
            await ExecuteAllMiddlewaresAsync(
                m => m.JobFailedAsync(jobContext, exception, cancellationToken),
                jobContext
            );
        }

        /// <summary>
        /// Called when a job fails and exceeds its maximum amount of retries.
        /// </summary>
        /// <param name="jobContext">The job that failed's context.</param>
        /// <param name="exception">The exception that occurred as a result of the job failing, if there is one.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async ValueTask JobFailedAndExceededRetriesAsync(
            JobContext jobContext,
            Exception? exception,
            CancellationToken cancellationToken
        )
        {
            await ExecuteAllMiddlewaresAsync(
                async m =>
                {
                    await m.JobFailedAndExceededRetriesAsync(jobContext, exception, cancellationToken);
                    return MiddlewareContinuation.Continue;
                },
                jobContext
            );
        }

        /// <summary>
        /// Runs the relevant middleware when a job is started. System based middlewares are ran first followed by any
        /// user provided ones.
        /// </summary>
        /// <param name="jobContext">The job that is being ran's context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async ValueTask JobStartedAsync(JobContext jobContext, CancellationToken cancellationToken)
        {
            await ExecuteAllMiddlewaresAsync(
                async m =>
                {
                    await m.JobStartedAsync(jobContext, cancellationToken);
                    return MiddlewareContinuation.Continue;
                },
                jobContext
            );
        }

        private async ValueTask ExecuteAllMiddlewaresAsync(
            Func<IJobMiddleware, ValueTask<MiddlewareContinuation>> toExecute,
            JobContext jobContext
        )
        {
            foreach (var middleware in SystemJobMiddlewares)
            {
                try
                {
                    await toExecute(middleware);
                }
                catch (Exception e)
                {
                    _logger.LogError(
                        jobContext, 
                        "Middleware {middlewareType} failed to execute.", 
                        e,
                        middleware.GetType());
                    
                    if (!middleware.ContinueExecutingMiddlewaresOnFailure)
                    {
                        _logger.LogDebug(
                            jobContext,
                            "Middleware {middlewareType} failed and is configured not to continue executing middlewares. Returning.",
                            middleware.GetType().Name
                        );

                        return;
                    }
                }
            }

            if (_serverContext.HasAdditionalDispatchedJobMiddlewares())
            {
                foreach (var middlewareType in _serverContext.AdditionalDispatchedJobMiddlewares)
                {
                    var activatedMiddleware = (IJobMiddleware) _serverContext.Activator.ActivateType(middlewareType);
                    
                    try
                    {
                        var cont = await toExecute(activatedMiddleware);
                        if (cont == MiddlewareContinuation.Abort)
                        {
                            break;
                        }   
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(
                            jobContext, 
                            "Consumer provided middleware {middlewareType} failed to execute.", 
                            e,
                            middlewareType.Name
                        );
                    
                        if (!activatedMiddleware.ContinueExecutingMiddlewaresOnFailure)
                        {
                            _logger.LogDebug(
                                jobContext,
                                "Middleware {middlewareType} failed and is configured not to continue executing middlewares. Returning.",
                                middlewareType.Name
                            );

                            return;
                        }
                    }
                }
            }
        }
    }
}