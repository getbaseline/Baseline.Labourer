using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Shared.Sqlite;
using Microsoft.Data.Sqlite;

namespace Baseline.Labourer.Store.Sqlite;

/// <summary>
/// SQLite backed reader.
/// </summary>
public class SqliteReader : BaseSqliteInteractor, IStoreReader
{
    public SqliteReader(string connectionString) : base(connectionString) { }

    /// <inheritdoc />
    public ValueTask<List<ScheduledJobDefinition>> GetScheduledJobsDueToRunBeforeDateAsync(
        DateTime before
    )
    {
        using var connection = NewConnection();
        var results = new List<ScheduledJobDefinition>();

        var scheduledJobsQueryCommand = new SqliteCommand(
            @"
                SELECT 
                    name,
                    cron_expression,
                    last_completed_at,
                    last_run_at,
                    next_run_at,
                    type,
                    parameters_type,
                    parameters,
                    created_at,
                    updated_at
                FROM bl_lb_scheduled_jobs
                WHERE next_run_at <= @Cutoff
            ",
            connection
        );
        scheduledJobsQueryCommand.Parameters.Add(
            new SqliteParameter("Cutoff", before.ToUniversalTime())
        );

        var reader = scheduledJobsQueryCommand.ExecuteReader();
        while (reader.Read())
        {
            results.Add(
                new ScheduledJobDefinition
                {
                    Name = reader.GetString(0),
                    CronExpression = reader.GetString(1),
                    LastCompletedDate = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                    LastRunDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    NextRunDate = reader.GetDateTime(4),
                    Type = reader.GetString(5),
                    HasParameters = !reader.IsDBNull(6),
                    ParametersType = reader.IsDBNull(6) ? null : reader.GetString(6),
                    SerializedParameters = reader.IsDBNull(6) ? null : reader.GetString(7),
                    CreatedAt = reader.GetDateTime(8),
                    UpdatedAt = reader.GetDateTime(9)
                }
            );
        }

        return ValueTask.FromResult(results);
    }
}
