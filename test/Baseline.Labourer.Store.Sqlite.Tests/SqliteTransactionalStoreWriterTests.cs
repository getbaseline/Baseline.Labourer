using System;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Tests;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Baseline.Labourer.Store.Sqlite.Tests;

public class SqliteTransactionalStoreWriterTests : BaseSqliteTest
{
    [Fact]
    public async Task It_Rolls_Back_Any_Changes_If_Commit_Not_Called()
    {
        // Arrange.
        CreateScheduledJob();
        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );

        // Act.
        await writer.DeleteScheduledJobAsync("scheduled-job");
        await writer.DisposeAsync();

        // Assert.
        var scheduledJobsCount = new SqliteCommand(
            "SELECT COUNT(1) FROM bl_lb_scheduled_jobs WHERE id = 'scheduled-job'",
            Connection
        ).ExecuteScalar();
        ((long)scheduledJobsCount!).Should().Be(1);
    }

    [Fact]
    public async Task It_Creates_A_Server()
    {
        // Arrange.
        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );

        // Act.
        await writer.CreateServerAsync(
            new ServerInstance { Hostname = "foo", Key = "bar" }
        );
        await writer.CommitAsync();

        // Assert.
        var serverRetrievalCommand = new SqliteCommand("SELECT * FROM bl_lb_servers", Connection);
        var reader = serverRetrievalCommand.ExecuteReader();
        reader.Read();

        reader.GetString(0).Should().Be("foo-bar");
    }

    [Fact]
    public async Task It_Creates_A_Server_Heartbeat()
    {
        // Arrange.
        var serverId = CreateServer();
        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );

        // Act.
        await writer.CreateServerHeartbeatAsync(serverId);
        await writer.CommitAsync();

        // Assert.
        var serverHeartbeatRetrievalCommand = new SqliteCommand(
            $"SELECT COUNT(1) FROM bl_lb_server_heartbeats WHERE server_id = '{serverId}'",
            Connection
        );
        ((long)serverHeartbeatRetrievalCommand.ExecuteScalar()!).Should().Be(1);
    }

    [Fact]
    public async Task It_Creates_A_Worker()
    {
        // Arrange.
        var serverId = CreateServer();
        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );

        // Act.
        await writer.CreateWorkerAsync(
            new Worker { ServerInstanceId = serverId, Id = Guid.NewGuid().ToString() }
        );
        await writer.CommitAsync();

        // Assert.
        var workerRetrievalCommand = new SqliteCommand(
            $"SELECT COUNT(1) FROM bl_lb_workers WHERE server_id = '{serverId}'",
            Connection
        );
        ((long)workerRetrievalCommand.ExecuteScalar()!).Should().Be(1);
    }

    [Fact]
    public async Task It_Creates_A_Dispatched_Job()
    {
        // Arrange.
        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );
        var dispatchedJob = new DispatchedJobDefinition
        {
            HasParameters = true,
            SerializedParameters = "foo",
            ParametersType = "bar",
            Type = "foo",
        };

        // Act.
        await writer.CreateDispatchedJobAsync(dispatchedJob);
        await writer.CommitAsync();

        // Assert.
        var dispatchedJobReader = new SqliteCommand(
            $"SELECT id, parameters_type, parameters, type FROM bl_lb_dispatched_jobs WHERE id = '{dispatchedJob.Id}'",
            Connection
        ).ExecuteReader();
        dispatchedJobReader.Read();
        dispatchedJobReader.GetString(0).Should().Be(dispatchedJob.Id);
        dispatchedJobReader.GetString(1).Should().Be(dispatchedJob.ParametersType);
        dispatchedJobReader.GetString(2).Should().Be(dispatchedJob.SerializedParameters);
        dispatchedJobReader.GetString(3).Should().Be(dispatchedJob.Type);
    }

    [Fact]
    public async Task It_Can_Create_And_Update_Dispatched_Jobs_Status_And_Retries()
    {
        // Arrange.
        var now = DateTime.UtcNow;
        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );
        var dispatchedJob = new DispatchedJobDefinition
        {
            HasParameters = true,
            SerializedParameters = "foo",
            ParametersType = "bar",
            Type = "foo",
        };

        // Act.
        await writer.CreateDispatchedJobAsync(dispatchedJob);
        await writer.UpdateJobRetriesAsync(dispatchedJob.Id, 10);
        await writer.UpdateJobStateAsync(
            dispatchedJob.Id,
            JobStatus.Complete,
            now
        );
        await writer.CommitAsync();

        // Assert.
        var dispatchedJobReader = new SqliteCommand(
            $"SELECT status, retries, finished_at FROM bl_lb_dispatched_jobs WHERE id = '{dispatchedJob.Id}'",
            Connection
        ).ExecuteReader();
        dispatchedJobReader.Read();
        dispatchedJobReader.GetString(0).Should().Be(JobStatus.Complete.ToString());
        dispatchedJobReader.GetInt32(1).Should().Be(10);
        dispatchedJobReader.GetDateTime(2).Should().Be(now);
    }

    [Fact]
    public async Task It_Creates_A_Scheduled_Job()
    {
        // Arrange.
        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );
        var scheduledJob = new ScheduledJobDefinition
        {
            Name = "foo",
            Type = "bar",
            ParametersType = "baz",
            SerializedParameters = "abc",
            CronExpression = "* * * * 4"
        };

        // Act.
        await writer.CreateOrUpdateScheduledJobAsync(scheduledJob);
        await writer.CommitAsync();

        // Assert.
        var scheduledJobReader = new SqliteCommand(
            $"SELECT id, name, type, parameters_type, parameters, cron_expression FROM bl_lb_scheduled_jobs WHERE id = '{scheduledJob.Id}'",
            Connection
        ).ExecuteReader();
        scheduledJobReader.Read();
        scheduledJobReader.GetString(0).Should().Be(scheduledJob.Id);
        scheduledJobReader.GetString(1).Should().Be(scheduledJob.Name);
        scheduledJobReader.GetString(2).Should().Be("bar");
        scheduledJobReader.GetString(3).Should().Be("baz");
        scheduledJobReader.GetString(4).Should().Be("abc");
        scheduledJobReader.GetString(5).Should().Be("* * * * 4");
    }

    [Fact]
    public async Task It_Updates_An_Existing_Scheduled_Job()
    {
        // Arrange.
        var createdAt = DateTime.Today;

        CreateScheduledJob(name: "a-scheduled-jobbo", createdAt: createdAt, updatedAt: createdAt);

        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );

        // Act.
        var scheduledJob = new ScheduledJobDefinition
        {
            Name = "foo",
            Type = "bar",
            ParametersType = "baz",
            SerializedParameters = "abc",
            CronExpression = "* * * * 4"
        };
        await writer.CreateOrUpdateScheduledJobAsync(scheduledJob);
        await writer.CommitAsync();

        // Assert.
        var scheduledJobReader = new SqliteCommand(
            $"SELECT id, name, type, parameters_type, parameters, cron_expression, created_at, updated_at FROM bl_lb_scheduled_jobs WHERE id = '{scheduledJob.Id}'",
            Connection
        ).ExecuteReader();
        scheduledJobReader.Read();
        scheduledJobReader.GetString(0).Should().Be(scheduledJob.Id);
        scheduledJobReader.GetString(1).Should().Be(scheduledJob.Name);
        scheduledJobReader.GetString(2).Should().Be("bar");
        scheduledJobReader.GetString(3).Should().Be("baz");
        scheduledJobReader.GetString(4).Should().Be("abc");
        scheduledJobReader.GetString(5).Should().Be("* * * * 4");
        scheduledJobReader.GetDateTime(6).Should().Be(scheduledJobReader.GetDateTime(7));
    }

    [Fact]
    public async Task It_Deletes_A_Scheduled_Job()
    {
        // Arrange.
        CreateScheduledJob();
        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );

        // Act.
        await writer.DeleteScheduledJobAsync("scheduled-job");
        await writer.CommitAsync();

        // Assert.
        var scheduledJobsCount = new SqliteCommand(
            "SELECT COUNT(1) FROM bl_lb_scheduled_jobs WHERE id = 'scheduled-job'",
            Connection
        ).ExecuteScalar();
        ((long)scheduledJobsCount!).Should().Be(0);
    }

    [Fact]
    public async Task It_Updates_A_Scheduled_Jobs_Next_Run_Date()
    {
        // Arrange.
        var nextRunDate = DateTime.Today.AddYears(1);
        CreateScheduledJob();
        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );

        // Act.
        await writer.UpdateScheduledJobNextRunDateAsync(
            "scheduled-job",
            nextRunDate
        );
        await writer.CommitAsync();

        // Assert.
        var scheduledJobReader = new SqliteCommand(
            $"SELECT next_run_at FROM bl_lb_scheduled_jobs WHERE name = 'scheduled-job'",
            Connection
        ).ExecuteReader();
        scheduledJobReader.Read();
        scheduledJobReader.GetDateTime(0).Should().Be(nextRunDate);
    }

    [Fact]
    public async Task It_Updates_A_Scheduled_Jobs_Last_Run_Date()
    {
        // Arrange.
        var lastRunDate = DateTime.Today.AddYears(1);
        CreateScheduledJob();
        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );

        // Act.
        await writer.UpdateScheduledJobLastRunDateAsync(
            "scheduled-job",
            lastRunDate
        );
        await writer.CommitAsync();

        // Assert.
        var scheduledJobReader = new SqliteCommand(
            $"SELECT last_run_at FROM bl_lb_scheduled_jobs WHERE name = 'scheduled-job'",
            Connection
        ).ExecuteReader();
        scheduledJobReader.Read();
        scheduledJobReader.GetDateTime(0).Should().Be(lastRunDate);
    }

    [Fact]
    public async Task It_Successfully_Logs_The_Job_Log_Message()
    {
        // Arrange.
        var exception = new ArgumentException("foo", "bar");
        var writer = new SqliteTransactionalStoreWriter(
            new TestDateTimeProvider(),
            ConnectionString
        );

        // Act.
        await writer.LogEntryForJobAsync(
            "1",
            LogLevel.Critical,
            "Foo bar.",
            exception
        );
        await writer.CommitAsync();

        // Assert.
        var logEntryRetrievalCommand = new SqliteCommand(
            $"SELECT id, job_id, log_level, message, exception FROM bl_lb_job_logs WHERE job_id = '1'",
            Connection
        ).ExecuteReader();

        logEntryRetrievalCommand.Read();

        logEntryRetrievalCommand.GetString(1).Should().Be("1");
        logEntryRetrievalCommand.GetString(2).Should().Be(LogLevel.Critical.ToString());
        logEntryRetrievalCommand.GetString(3).Should().Be("Foo bar.");
        logEntryRetrievalCommand.GetString(4).Should().Contain("foo");
    }

    private string CreateServer()
    {
        var id = Guid.NewGuid();

        var createServerCommand = new SqliteCommand(
            @"
                INSERT INTO bl_lb_servers (id, hostname, key, created_at)
                VALUES (@Id, @Hostname, @Key, @Now)
            ",
            Connection
        );
        createServerCommand.Parameters.Add(new SqliteParameter("@Id", $"hostname-{id}"));
        createServerCommand.Parameters.Add(new SqliteParameter("@Hostname", "hostname"));
        createServerCommand.Parameters.Add(new SqliteParameter("@Key", id));
        createServerCommand.Parameters.Add(new SqliteParameter("@Now", DateTime.UtcNow));
        createServerCommand.ExecuteNonQuery();

        return $"hostname-{id}";
    }
}
