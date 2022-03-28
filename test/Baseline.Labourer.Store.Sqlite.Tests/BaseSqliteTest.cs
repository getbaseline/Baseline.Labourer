using System;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Baseline.Labourer.Store.Sqlite.Tests;

public abstract class BaseSqliteTest : IAsyncLifetime
{
    private readonly Guid _databaseId = Guid.NewGuid();
    protected readonly string ConnectionString;
    protected readonly SqliteConnection Connection;

    protected BaseSqliteTest()
    {
        ConnectionString = $"Data Source={_databaseId};Mode=Memory;Cache=Shared";
        Connection = new SqliteConnection(ConnectionString);
        Connection.Open();
    }

    public Task DisposeAsync()
    {
        Connection.Dispose();
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        await new SqliteStore(new DateTimeProvider(), ConnectionString).BootstrapAsync();
        await new SqliteQueue(new DateTimeProvider(), ConnectionString).BootstrapAsync();
    }

    protected void CreateScheduledJob(
        string name = "scheduled-job",
        string cronExpression = "* * * * *",
        DateTime? lastCompleted = null,
        DateTime? lastRun = null,
        DateTime? nextRun = null,
        string type = "scheduled-job-type",
        string parametersType = "scheduled-job-parameters-type",
        string parameters = "scheduled-job-parameters",
        DateTime? createdAt = null,
        DateTime? updatedAt = null
    )
    {
        var command = new SqliteCommand(
            @"
                INSERT INTO bl_lb_scheduled_jobs (
                    id,
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
                )
                VALUES (
                    @Id,
                    @Name, 
                    @CronExpression, 
                    @LastCompleted, 
                    @LastRun, 
                    @NextRun, 
                    @Type, 
                    @ParametersType, 
                    @Parameters, 
                    @CreatedAt, 
                    @UpdatedAt
                );
            ",
            Connection
        );
        command.Parameters.Add(new SqliteParameter("@Id", name));
        command.Parameters.Add(new SqliteParameter("@Name", name));
        command.Parameters.Add(new SqliteParameter("@CronExpression", cronExpression));
        command.Parameters.Add(
            lastCompleted != null
              ? new SqliteParameter("@LastCompleted", lastCompleted)
              : new SqliteParameter("@LastCompleted", DBNull.Value)
        );
        
        command.Parameters.Add(
            lastRun != null
              ? new SqliteParameter("@LastRun", lastRun)
              : new SqliteParameter("@LastRun", DBNull.Value)
        );
        command.Parameters.Add(
            new SqliteParameter("@NextRun", nextRun ?? DateTime.UtcNow.AddDays(1))
        );
        command.Parameters.Add(new SqliteParameter("@Type", type));
        command.Parameters.Add(new SqliteParameter("@ParametersType", parametersType));
        command.Parameters.Add(new SqliteParameter("@Parameters", parameters));
        command.Parameters.Add(new SqliteParameter("@CreatedAt", createdAt ?? DateTime.UtcNow));
        command.Parameters.Add(new SqliteParameter("@UpdatedAt", updatedAt ?? DateTime.UtcNow));

        command.ExecuteNonQuery();
    }
}
