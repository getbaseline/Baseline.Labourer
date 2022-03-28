using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Microsoft.Data.Sqlite;

namespace Baseline.Labourer;

/// <summary>
/// SqliteQueue is a queue implementation that utilises a SQLite backing store.
/// </summary>
public class SqliteQueue : BaseSqliteBootstrapper<SqliteQueue>, IQueue
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public SqliteQueue(string connectionString) : this(new DateTimeProvider(), connectionString) { }

    public SqliteQueue(IDateTimeProvider dateTimeProvider, string connectionString)
        : base(connectionString)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc />
    public async Task EnqueueAsync<T>(
        T messageToQueue,
        TimeSpan? visibilityDelay,
        CancellationToken cancellationToken
    )
    {
        using var connection = NewConnection();

        var enqueueCommand = new SqliteCommand(
            @"
                INSERT INTO bl_lb_queue (message, hidden_until, created_at)
                VALUES (@Message, @HiddenUntil, @Now)
            ",
            connection
        );
        enqueueCommand.Parameters.Add(
            new SqliteParameter(
                "@Message",
                await SerializationUtils.SerializeToStringAsync(messageToQueue, cancellationToken)
            )
        );
        enqueueCommand.Parameters.Add(
            new SqliteParameter(
                "@HiddenUntil",
                visibilityDelay == null
                  ? DBNull.Value
                  : _dateTimeProvider.UtcNow().Add(visibilityDelay.Value)
            )
        );
        enqueueCommand.Parameters.Add(new SqliteParameter("@Now", _dateTimeProvider.UtcNow()));

        enqueueCommand.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public ValueTask<QueuedJob?> DequeueAsync(CancellationToken cancellationToken)
    {
        using var connection = NewConnection();
        var transaction = connection.BeginTransaction();

        var (hasDequeuedJob, messageId, queuedJob) = Dequeue(connection, transaction);
        if (!hasDequeuedJob)
        {
            return ValueTask.FromResult((QueuedJob?)null);
        }

        UpdateHiddenUntilOfMessage(messageId, connection, transaction);
        transaction.Commit();

        return ValueTask.FromResult(queuedJob);
    }

    /// <inheritdoc />
    public ValueTask DeleteMessageAsync(string messageId, CancellationToken cancellationToken)
    {
        using var connection = NewConnection();

        var deletionCommand = new SqliteCommand(
            "DELETE FROM bl_lb_queue WHERE id = @Id",
            connection
        );
        deletionCommand.Parameters.Add(new SqliteParameter("@Id", long.Parse(messageId)));
        deletionCommand.ExecuteNonQuery();

        return ValueTask.CompletedTask;
    }

    private (bool, long, QueuedJob?) Dequeue(
        SqliteConnection connection,
        SqliteTransaction transaction
    )
    {
        var retrievalCommand = new SqliteCommand(
            @"
                SELECT id, message
                FROM bl_lb_queue
                WHERE hidden_until IS NULL OR
                      hidden_until <= @Now
                ORDER BY created_at ASC
                LIMIT 1
            ",
            connection,
            transaction
        );
        retrievalCommand.Parameters.Add(new SqliteParameter("@Now", _dateTimeProvider.UtcNow()));

        using var retrievalReader = retrievalCommand.ExecuteReader();
        if (!retrievalReader.Read())
        {
            return (false, 0, null);
        }

        var messageId = retrievalReader.GetInt64(0);
        return (
            true,
            messageId,
            new QueuedJob
            {
                MessageId = messageId.ToString(),
                SerializedDefinition = retrievalReader.GetString(1)
            }
        );
    }

    private void UpdateHiddenUntilOfMessage(
        long id,
        SqliteConnection connection,
        SqliteTransaction transaction
    )
    {
        var updateHiddenUntilCommand = new SqliteCommand(
            @"
                UPDATE bl_lb_queue
                SET hidden_until = @HiddenUntil
                WHERE id = @Id
            ",
            connection,
            transaction
        );
        updateHiddenUntilCommand.Parameters.Add(new SqliteParameter("@Id", id));
        updateHiddenUntilCommand.Parameters.Add(
            new SqliteParameter("@HiddenUntil", _dateTimeProvider.UtcNow().AddSeconds(30))
        );
        updateHiddenUntilCommand.ExecuteNonQuery();
    }
}
