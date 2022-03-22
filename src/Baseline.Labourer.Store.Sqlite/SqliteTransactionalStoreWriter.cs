using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;

namespace Baseline.Labourer;

/// <summary>
/// An <see cref="ITransactionalStoreWriter"/> implementation that persists its information in a SQLite database.
/// </summary>
public class SqliteTransactionalStoreWriter : BaseSqliteInteractor, ITransactionalStoreWriter
{
    public SqliteTransactionalStoreWriter(string connectionString) : base(connectionString) { }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask CommitAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask CreateServerAsync(
        ServerInstance serverInstance,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask CreateServerHeartbeatAsync(
        string serverId,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask CreateWorkerAsync(Worker worker, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask CreateDispatchedJobAsync(
        DispatchedJobDefinition definition,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask CreateOrUpdateScheduledJobAsync(
        ScheduledJobDefinition scheduledJobDefinition,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask DeleteScheduledJobAsync(string id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask UpdateJobRetriesAsync(
        string jobId,
        uint retries,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask UpdateJobStateAsync(
        string jobId,
        JobStatus jobStatus,
        DateTime? finishedDate,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask UpdateScheduledJobNextRunDateAsync(
        string jobId,
        DateTime nextRunDate,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ValueTask UpdateScheduledJobLastRunDateAsync(
        string jobId,
        DateTime? lastRunDate,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }
}
