using System;
using System.Threading.Tasks;

namespace Baseline.Labourer.Server;

/// <summary>
/// Helper base class for dispatched job middlewares. Allows relevant implementations to only implement middleware
/// functions that they care about.
/// </summary>
public abstract class JobMiddleware : IJobMiddleware
{
    /// <inheritdoc />
    public virtual bool ContinueExecutingMiddlewaresOnFailure => true;

    /// <inheritdoc />
    public virtual ValueTask JobCompletedAsync(JobContext jobContext)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public virtual ValueTask<MiddlewareContinuation> JobFailedAsync(
        JobContext jobContext,
        Exception? exception
    )
    {
        return new ValueTask<MiddlewareContinuation>(MiddlewareContinuation.Continue);
    }

    /// <inheritdoc />
    public virtual ValueTask JobFailedAndExceededRetriesAsync(
        JobContext jobContext,
        Exception? exception
    )
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public virtual ValueTask JobStartedAsync(JobContext jobContext)
    {
        return new ValueTask();
    }
}
