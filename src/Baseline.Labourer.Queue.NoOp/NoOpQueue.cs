using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;

namespace Baseline.Labourer;

/// <summary>
/// NoOpQueue is an <see cref="IQueue"/> implementation that literally does nothing. Useful for testing if you
/// don't actually want jobs to be dispatched and potentially ran.
/// </summary>
public class NoOpQueue : IQueue
{
    /// <inheritdoc />
    public ValueTask BootstrapAsync()
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public Task EnqueueAsync<T>(
        T messageToQueue,
        TimeSpan? visibilityDelay,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<QueuedJob?> DequeueAsync(CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(null as QueuedJob);
    }

    /// <inheritdoc />
    public ValueTask DeleteMessageAsync(string messageId, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}
