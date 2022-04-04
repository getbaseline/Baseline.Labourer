using System;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Store.NoOp;

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
    public ValueTask CommitAsync()
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CreateServerAsync(ServerInstance serverInstance)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CreateServerHeartbeatAsync(string serverId)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CreateWorkerAsync(Worker worker)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CreateDispatchedJobAsync(DispatchedJobDefinition definition)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CreateOrUpdateScheduledJobAsync(ScheduledJobDefinition scheduledJobDefinition)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask DeleteScheduledJobAsync(string id)
    {
        return new ValueTask();
    }

    public ValueTask LogEntryForJobAsync(
        string jobId,
        LogLevel logLevel,
        string message,
        Exception? exception
    )
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask UpdateJobRetriesAsync(string jobId, uint retries)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask UpdateJobStateAsync(string jobId, JobStatus jobStatus, DateTime? finishedDate)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask UpdateScheduledJobNextRunDateAsync(string jobId, DateTime nextRunDate)
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask UpdateScheduledJobLastRunDateAsync(string jobId, DateTime? lastRunDate)
    {
        return new ValueTask();
    }
}
