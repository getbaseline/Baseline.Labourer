using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;

namespace Baseline.Labourer;

/// <summary>
/// NoOpTransactionalStoreWriter is an <see cref="ITransactionalStoreWriter"/> that does literally nothing.
/// </summary>
public class NoOpTransactionalStoreWriter : ITransactionalStoreWriter
{
    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CommitAsync(CancellationToken cancellationToken)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CreateServerAsync(ServerInstance serverInstance, CancellationToken cancellationToken)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CreateServerHeartbeatAsync(string serverId, CancellationToken cancellationToken)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CreateWorkerAsync(Worker worker, CancellationToken cancellationToken)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CreateDispatchedJobAsync(
        DispatchedJobDefinition definition,
        CancellationToken cancellationToken
    )
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CreateOrUpdateScheduledJobAsync(
        ScheduledJobDefinition scheduledJobDefinition,
        CancellationToken cancellationToken
    )
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask DeleteScheduledJobAsync(string id, CancellationToken cancellationToken)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask UpdateJobRetriesAsync(string jobId, uint retries, CancellationToken cancellationToken)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask UpdateJobStateAsync(
        string jobId,
        JobStatus jobStatus,
        DateTime? finishedDate,
        CancellationToken cancellationToken = default
    )
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask UpdateScheduledJobNextRunDateAsync(
        string jobId,
        DateTime nextRunDate,
        CancellationToken cancellationToken
    )
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask UpdateScheduledJobLastRunDateAsync(
        string jobId,
        DateTime? lastRunDate,
        CancellationToken cancellationToken
    )
    {
        return new ValueTask();
    }
}