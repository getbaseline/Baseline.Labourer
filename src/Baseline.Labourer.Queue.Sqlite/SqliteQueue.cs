using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;

namespace Baseline.Labourer;

/// <summary>
/// SqliteQueue is a queue implementation that utilises a SQLite backing store.
/// </summary>
public class SqliteQueue : IQueue
{
    /// <inheritdoc />
    public ValueTask BootstrapAsync()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task EnqueueAsync<T>(T messageToQueue, TimeSpan? visibilityDelay, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<QueuedJob?> DequeueAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task DeleteMessageAsync(string messageId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}