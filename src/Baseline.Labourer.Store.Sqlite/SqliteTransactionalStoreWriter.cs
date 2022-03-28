﻿using System;
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
        var createWorkerCommand = new SqliteCommand(
            @"
                INSERT INTO bl_lb_workers (id, server_id, created_at)
                VALUES (@Id, @ServerId, @Now)
            ",
            _connection,
            _transaction
        );
        createWorkerCommand.Parameters.Add(new SqliteParameter("@Id", worker.Id));
        createWorkerCommand.Parameters.Add(
            new SqliteParameter("@ServerId", worker.ServerInstanceId)
        );
        createWorkerCommand.Parameters.Add(new SqliteParameter("@Now", _dateTimeProvider.UtcNow()));
        createWorkerCommand.ExecuteNonQuery();

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask CreateDispatchedJobAsync(
        DispatchedJobDefinition definition,
        CancellationToken cancellationToken
    )
    {
        var createDispatchedJobCommand = new SqliteCommand(
            @"
                INSERT INTO bl_lb_dispatched_jobs (id, status, type, parameters_type, parameters, created_at, updated_at)
                VALUES (@Id, @Status, @Type, @ParametersType, @Parameters, @Now, @Now)
            ",
            _connection,
            _transaction
        );
        createDispatchedJobCommand.Parameters.Add(new SqliteParameter("@Id", definition.Id));
        createDispatchedJobCommand.Parameters.Add(
            new SqliteParameter("@Status", (int)JobStatus.Created)
        );
        createDispatchedJobCommand.Parameters.Add(new SqliteParameter("@Type", definition.Type));
        createDispatchedJobCommand.Parameters.Add(
            new SqliteParameter(
                "@ParametersType",
                definition.ParametersType == null ? DBNull.Value : definition.ParametersType
            )
        );
        createDispatchedJobCommand.Parameters.Add(
            new SqliteParameter(
                "@Parameters",
                definition.SerializedParameters == null
                  ? DBNull.Value
                  : definition.SerializedParameters
            )
        );
        createDispatchedJobCommand.Parameters.Add(
            new SqliteParameter("@Now", _dateTimeProvider.UtcNow())
        );

        createDispatchedJobCommand.ExecuteNonQuery();

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask CreateOrUpdateScheduledJobAsync(
        ScheduledJobDefinition scheduledJobDefinition,
        CancellationToken cancellationToken
    )
    {
        var existsCommand = new SqliteCommand(
            "SELECT COUNT(1) FROM bl_lb_scheduled_jobs WHERE id = @Id",
            _connection,
            _transaction
        );
        existsCommand.Parameters.Add(new SqliteParameter("@Id", scheduledJobDefinition.Id));
        var exists = ((long)existsCommand.ExecuteScalar()!) > 0;

        SqliteCommand updateOrCreateCommand;

        if (exists)
        {
            updateOrCreateCommand = new SqliteCommand(
                @"
                    UPDATE 
                        bl_lb_scheduled_jobs
                    SET 
                        name = @Name,
                        cron_expression = @CronExpression,
                        type = @Type,
                        parameters_type = @ParametersType,
                        parameters = @Parameters,
                        updated_at = @Now
                    WHERE
                        id = @Id
                ",
                _connection,
                _transaction
            );
        }
        else
        {
            updateOrCreateCommand = new SqliteCommand(
                @"
                    INSERT INTO bl_lb_scheduled_jobs 
                        (
                            id,
                            name,
                            cron_expression,
                            type,
                            parameters_type,
                            parameters,
                            created_at,
                            updated_at
                        )
                    VALUES 
                        (
                            @Id,
                            @Name,
                            @CronExpression,
                            @Type,
                            @ParametersType,
                            @Parameters,
                            @Now,
                            @Now
                        )
                ",
                _connection,
                _transaction
            );
        }

        updateOrCreateCommand.Parameters.Add(new SqliteParameter("@Id", scheduledJobDefinition.Id));
        updateOrCreateCommand.Parameters.Add(
            new SqliteParameter("@Name", scheduledJobDefinition.Name)
        );
        updateOrCreateCommand.Parameters.Add(
            new SqliteParameter("@Type", scheduledJobDefinition.Type)
        );
        updateOrCreateCommand.Parameters.Add(
            new SqliteParameter("@CronExpression", scheduledJobDefinition.CronExpression)
        );
        updateOrCreateCommand.Parameters.Add(
            new SqliteParameter(
                "@ParametersType",
                scheduledJobDefinition.ParametersType == null
                  ? DBNull.Value
                  : scheduledJobDefinition.ParametersType
            )
        );
        updateOrCreateCommand.Parameters.Add(
            new SqliteParameter(
                "@Parameters",
                scheduledJobDefinition.SerializedParameters == null
                  ? DBNull.Value
                  : scheduledJobDefinition.SerializedParameters
            )
        );
        updateOrCreateCommand.Parameters.Add(
            new SqliteParameter("@Now", _dateTimeProvider.UtcNow())
        );

        updateOrCreateCommand.ExecuteNonQuery();

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DeleteScheduledJobAsync(string id, CancellationToken cancellationToken)
    {
        var deleteScheduledJobCommand = new SqliteCommand(
            @"
                DELETE FROM bl_lb_scheduled_jobs
                WHERE id = @Id
            ",
            _connection,
            _transaction
        );
        deleteScheduledJobCommand.Parameters.Add(new SqliteParameter("@Id", id));
        deleteScheduledJobCommand.ExecuteNonQuery();

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask UpdateJobRetriesAsync(
        string jobId,
        uint retries,
        CancellationToken cancellationToken
    )
    {
        var updateJobRetriesCommand = new SqliteCommand(
            @"
                UPDATE bl_lb_dispatched_jobs
                SET retries = @Retries, updated_at = @Now
                WHERE id = @Id
            ",
            _connection,
            _transaction
        );
        updateJobRetriesCommand.Parameters.Add(new SqliteParameter("@Retries", retries));
        updateJobRetriesCommand.Parameters.Add(
            new SqliteParameter("@Now", _dateTimeProvider.UtcNow())
        );
        updateJobRetriesCommand.Parameters.Add(new SqliteParameter("@Id", jobId));
        updateJobRetriesCommand.ExecuteNonQuery();

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask UpdateJobStateAsync(
        string jobId,
        JobStatus jobStatus,
        DateTime? finishedDate,
        CancellationToken cancellationToken = default
    )
    {
        var updateJobStateCommand = new SqliteCommand(
            @"
                UPDATE bl_lb_dispatched_jobs
                SET status = @Status, updated_at = @Now, finished_at = @FinishedAt
                WHERE id = @Id
            ",
            _connection,
            _transaction
        );
        updateJobStateCommand.Parameters.Add(new SqliteParameter("@Status", jobStatus.ToString()));
        updateJobStateCommand.Parameters.Add(
            new SqliteParameter(
                "@FinishedAt",
                finishedDate == null ? DBNull.Value : finishedDate.Value
            )
        );
        updateJobStateCommand.Parameters.Add(
            new SqliteParameter("@Now", _dateTimeProvider.UtcNow())
        );
        updateJobStateCommand.Parameters.Add(new SqliteParameter("@Id", jobId));
        updateJobStateCommand.ExecuteNonQuery();

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask UpdateScheduledJobNextRunDateAsync(
        string jobId,
        DateTime nextRunDate,
        CancellationToken cancellationToken
    )
    {
        var updateNextRunDateCommand = new SqliteCommand(
            @"
                UPDATE bl_lb_scheduled_jobs
                SET next_run_at = @NextRun, updated_at = @Now
                WHERE id = @Id
            ",
            _connection,
            _transaction
        );
        updateNextRunDateCommand.Parameters.Add(new SqliteParameter("@Id", jobId));
        updateNextRunDateCommand.Parameters.Add(new SqliteParameter("@NextRun", nextRunDate));
        updateNextRunDateCommand.Parameters.Add(
            new SqliteParameter("@Now", _dateTimeProvider.UtcNow())
        );
        updateNextRunDateCommand.ExecuteNonQuery();

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask UpdateScheduledJobLastRunDateAsync(
        string jobId,
        DateTime? lastRunDate,
        CancellationToken cancellationToken
    )
    {
        var updateLastRunDateCommand = new SqliteCommand(
            @"
                UPDATE bl_lb_scheduled_jobs
                SET last_run_at = @LastRun, updated_at = @Now
                WHERE id = @Id
            ",
            _connection,
            _transaction
        );
        updateLastRunDateCommand.Parameters.Add(new SqliteParameter("@Id", jobId));
        updateLastRunDateCommand.Parameters.Add(new SqliteParameter("@LastRun", lastRunDate));
        updateLastRunDateCommand.Parameters.Add(
            new SqliteParameter("@Now", _dateTimeProvider.UtcNow())
        );
        updateLastRunDateCommand.ExecuteNonQuery();

        return ValueTask.CompletedTask;
    }
}
