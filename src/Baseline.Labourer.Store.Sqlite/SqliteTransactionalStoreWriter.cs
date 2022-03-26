using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Microsoft.Data.Sqlite;

namespace Baseline.Labourer;

/// <summary>
/// An <see cref="ITransactionalStoreWriter"/> implementation that persists its information in a SQLite database.
/// </summary>
public class SqliteTransactionalStoreWriter : BaseSqliteInteractor, ITransactionalStoreWriter
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly SqliteConnection _connection;
    private readonly SqliteTransaction _transaction;

    public SqliteTransactionalStoreWriter(
        IDateTimeProvider dateTimeProvider,
        string connectionString
    ) : base(connectionString)
    {
        _dateTimeProvider = dateTimeProvider;
        _connection = NewConnection();
        _transaction = _connection.BeginTransaction();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _transaction.Dispose();
        _connection.Dispose();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask CommitAsync(CancellationToken cancellationToken)
    {
        _transaction.Commit();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask CreateServerAsync(
        ServerInstance serverInstance,
        CancellationToken cancellationToken
    )
    {
        var createServerCommand = new SqliteCommand(
            @"
                INSERT INTO bl_lb_servers (id, hostname, key, created_at)
                VALUES (@Id, @Hostname, @Key, @Now)
            ",
            _connection,
            _transaction
        );
        createServerCommand.Parameters.Add(new SqliteParameter("@Id", serverInstance.Id));
        createServerCommand.Parameters.Add(
            new SqliteParameter("@Hostname", serverInstance.Hostname)
        );
        createServerCommand.Parameters.Add(new SqliteParameter("@Key", serverInstance.Key));
        createServerCommand.Parameters.Add(new SqliteParameter("@Now", _dateTimeProvider.UtcNow()));

        createServerCommand.ExecuteNonQuery();

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask CreateServerHeartbeatAsync(
        string serverId,
        CancellationToken cancellationToken
    )
    {
        var createHeartbeatCommand = new SqliteCommand(
            @"
                INSERT INTO bl_lb_server_heartbeats (server_id, created_at)
                VALUES (@ServerId, @Now)
            ",
            _connection,
            _transaction
        );
        createHeartbeatCommand.Parameters.Add(new SqliteParameter("@ServerId", serverId));
        createHeartbeatCommand.Parameters.Add(
            new SqliteParameter("@Now", _dateTimeProvider.UtcNow())
        );

        createHeartbeatCommand.ExecuteNonQuery();

        return ValueTask.CompletedTask;
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
