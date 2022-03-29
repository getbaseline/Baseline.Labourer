using System;
using System.Threading.Tasks;

namespace Baseline.Labourer.Server;

/// <summary>
/// Represents what all dispatched job middlewares must implement.
/// </summary>
public interface IJobMiddleware
{
    /// <summary>
    /// Gets whether or not further middlewares should be executed on failures of this one.
    /// </summary>
    bool ContinueExecutingMiddlewaresOnFailure { get; }

    /// <summary>
    /// Called when a job is completed.
    /// </summary>
    /// <param name="jobContext">The job that is completed's context.</param>
    ValueTask JobCompletedAsync(JobContext jobContext);

    /// <summary>
    /// Called when a job fails.
    /// </summary>
    /// <param name="jobContext">The job that failed's context.</param>
    /// <param name="exception">The exception that occurred as a result of the job failing, if there is one.</param>
    /// <returns>
    /// A <see cref="MiddlewareContinuation"/> value indicating whether the rest of the middlewares in the chain should run.
    /// </returns>
    ValueTask<MiddlewareContinuation> JobFailedAsync(JobContext jobContext, Exception? exception);

    /// <summary>
    /// Called when a job fails and exceeds its maximum amount of retries.
    /// </summary>
    /// <param name="jobContext">The job that failed's context.</param>
    /// <param name="exception">The exception that occurred as a result of the job failing, if there is one.</param>
    ValueTask JobFailedAndExceededRetriesAsync(JobContext jobContext, Exception? exception);

    /// <summary>
    /// Called when a job is started.
    /// </summary>
    /// <param name="jobContext">The job that is being started's context.</param>
    ValueTask JobStartedAsync(JobContext jobContext);
}
