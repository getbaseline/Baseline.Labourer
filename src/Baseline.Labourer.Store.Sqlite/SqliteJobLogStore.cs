using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer;

/// <summary>
/// SQLite backed job log store.
/// </summary>
public class SqliteJobLogStore : BaseSqliteInteractor, IJobLogStore
{
    public SqliteJobLogStore(string connectionString) : base(connectionString) { }

    /// <inheritdoc />
    public void LogEntryForJob(
        string jobId,
        LogLevel logLevel,
        string message,
        Exception? exception
    )
    {
        using var connection = NewConnection();
        using var transaction = connection.BeginTransaction();

        var logEntryCommand = new SqliteCommand(
            @"
                INSERT INTO bl_lb_job_logs (job_id, log_level, message, exception, created_at)
                VALUES (@JobId, @LogLevel, @Message, @Exception, @At)
            ",
            connection,
            transaction
        );
        logEntryCommand.Parameters.Add(new SqliteParameter("@JobId", jobId));
        logEntryCommand.Parameters.Add(new SqliteParameter("@LogLevel", logLevel.ToString()));
        logEntryCommand.Parameters.Add(new SqliteParameter("@Message", message));
        logEntryCommand.Parameters.Add(
            new SqliteParameter(
                "@Exception",
                exception == null
                  ? DBNull.Value
                  : JsonSerializer.Serialize(
                        exception,
                        new JsonSerializerOptions
                        {
                            ReferenceHandler = ReferenceHandler.IgnoreCycles
                        }
                    )
            )
        );
        logEntryCommand.Parameters.Add(new SqliteParameter("@At", DateTime.UtcNow));
        logEntryCommand.ExecuteNonQuery();

        transaction.Commit();
    }
}
