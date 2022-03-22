using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Baseline.Labourer.Store.Sqlite.Tests;

public class SqliteReaderTests : BaseSqliteTest
{
    private readonly SqliteReader _sqliteReader;

    public SqliteReaderTests()
    {
        _sqliteReader = new SqliteReader(ConnectionString);
    }

    [Fact]
    public async Task It_Retrieves_Scheduled_Jobs_Correctly()
    {
        // Arrange.
        var beforeDate = DateTime.UtcNow;

        CreateScheduledJob(name: "before", nextRun: DateTime.UtcNow.AddDays(-7));
        CreateScheduledJob(name: "same", nextRun: beforeDate);
        CreateScheduledJob(name: "after", nextRun: DateTime.UtcNow.AddDays(7));

        // Act.
        var result = await _sqliteReader.GetScheduledJobsDueToRunBeforeDateAsync(
            beforeDate,
            CancellationToken.None
        );

        // Assert.
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().ContainSingle(r => r.Name == "before");
        result.Should().ContainSingle(r => r.Name == "same");
        result.Should().NotContain(r => r.Name == "after");
    }

    private void CreateScheduledJob(
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
                    name, 
                    cron_expression, 
                    last_completed, 
                    last_run, 
                    next_run, 
                    type, 
                    parameters_type, 
                    parameters, 
                    created_at, 
                    updated_at
                )
                VALUES (
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
