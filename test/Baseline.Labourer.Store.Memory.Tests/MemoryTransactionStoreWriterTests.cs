using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Tests;
using FluentAssertions;
using Xunit;

namespace Baseline.Labourer.Store.Memory.Tests;

public class MemoryTransactionStoreWriterTests
{
    private readonly TestMemoryStoreDataContainer _memoryStoreDataContainer =
        new TestMemoryStoreDataContainer();
    private readonly MemoryStoreWriterTransactionManager _transactionManager;

    public MemoryTransactionStoreWriterTests()
    {
        _transactionManager = new MemoryStoreWriterTransactionManager(_memoryStoreDataContainer);
    }

    [Fact]
    public async Task It_Can_Create_A_Server()
    {
        // Arrange.
        await using var writer = _transactionManager.BeginTransaction();

        // Act.
        await writer.CreateServerAsync(
            new ServerInstance { Hostname = "foo", Key = "bar" },
            CancellationToken.None
        );
        await writer.CommitAsync(CancellationToken.None);

        // Assert.
        _memoryStoreDataContainer.Servers.Should().ContainSingle(s => s.Id == "foo-bar");
    }

    [Fact]
    public async Task It_Can_Create_A_Server_Heartbeat()
    {
        // Arrange.
        await using var writer = _transactionManager.BeginTransaction();

        // Act.
        await writer.CreateServerHeartbeatAsync("abc", CancellationToken.None);
        await writer.CommitAsync(CancellationToken.None);

        // Assert.
        _memoryStoreDataContainer.ServerHeartbeats["abc"].Should().HaveCount(1);
    }

    [Fact]
    public async Task It_Can_Create_A_Worker()
    {
        // Arrange.
        await using var writer = _transactionManager.BeginTransaction();

        // Act.
        await writer.CreateWorkerAsync(
            new Worker { Id = "foo", ServerInstanceId = "foo" },
            CancellationToken.None
        );
        await writer.CommitAsync(CancellationToken.None);

        // Assert.
        _memoryStoreDataContainer.ServerWorkers["foo"].Should().ContainSingle(w => w.Id == "foo");
    }

    [Fact]
    public async Task It_Can_Create_A_Scheduled_Job()
    {
        // Arrange.
        await using var writer = _transactionManager.BeginTransaction();

        // Act.
        await writer.CreateOrUpdateScheduledJobAsync(
            new ScheduledJobDefinition { Name = "scheduled-job", CronExpression = "abc" },
            CancellationToken.None
        );
        await writer.CommitAsync(CancellationToken.None);

        // Assert.
        _memoryStoreDataContainer.ScheduledJobs["scheduled-job:scheduled-job"].CronExpression
            .Should()
            .Be("abc");
    }

    [Fact]
    public async Task It_Can_Update_A_Scheduled_Jobs_Next_Run_Date()
    {
        // Arrange.
        var scheduledJob = new ScheduledJobDefinition
        {
            Name = "next-run-date",
            CronExpression = "abc",
            NextRunDate = DateTime.UtcNow.Date.AddDays(-3)
        };
        _memoryStoreDataContainer.ScheduledJobs.Add(scheduledJob.Id, scheduledJob);

        await using var writer = _transactionManager.BeginTransaction();

        // Act.
        await writer.UpdateScheduledJobNextRunDateAsync(
            scheduledJob.Id,
            DateTime.UtcNow.Date.AddDays(7),
            CancellationToken.None
        );
        await writer.CommitAsync(CancellationToken.None);

        // Assert.
        _memoryStoreDataContainer.ScheduledJobs[scheduledJob.Id].NextRunDate
            .Should()
            .Be(DateTime.UtcNow.Date.AddDays(7));
    }

    [Fact]
    public async Task It_Can_Update_A_Scheduled_Jobs_Last_Run_Date()
    {
        // Arrange.
        var scheduledJob = new ScheduledJobDefinition
        {
            Name = "last-run-date",
            CronExpression = "abc",
            LastRunDate = DateTime.UtcNow.Date.AddDays(-3)
        };
        _memoryStoreDataContainer.ScheduledJobs.Add(scheduledJob.Id, scheduledJob);

        await using var writer = _transactionManager.BeginTransaction();

        // Act.
        await writer.UpdateScheduledJobLastRunDateAsync(
            scheduledJob.Id,
            DateTime.UtcNow.Date.AddDays(7),
            CancellationToken.None
        );
        await writer.CommitAsync(CancellationToken.None);

        // Assert.
        _memoryStoreDataContainer.ScheduledJobs[scheduledJob.Id].LastRunDate
            .Should()
            .Be(DateTime.UtcNow.Date.AddDays(7));
    }

    [Fact]
    public async Task It_Can_Create_A_Dispatched_Job()
    {
        // Arrange.
        await using var writer = _transactionManager.BeginTransaction();

        // Act.
        await writer.CreateDispatchedJobAsync(
            new DispatchedJobDefinition { Id = "bar" },
            CancellationToken.None
        );
        await writer.CommitAsync(CancellationToken.None);

        // Assert.
        _memoryStoreDataContainer.DispatchedJobs.Should().ContainSingle(j => j.Id == "bar");
    }

    [Fact]
    public async Task It_Can_Update_A_Dispatched_Jobs_Status()
    {
        // Arrange.
        var jobDefinition = new DispatchedJobDefinition { Id = "abc" };
        _memoryStoreDataContainer.DispatchedJobs.Add(jobDefinition);

        await using var writer = _transactionManager.BeginTransaction();

        // Act.
        await writer.UpdateJobStateAsync(
            jobDefinition.Id,
            JobStatus.Complete,
            DateTime.UtcNow.Date,
            CancellationToken.None
        );
        await writer.CommitAsync(CancellationToken.None);

        // Assert.
        _memoryStoreDataContainer.DispatchedJobs
            .Should()
            .ContainSingle(
                j =>
                    j.Id == jobDefinition.Id
                    && j.Status == JobStatus.Complete
                    && j.FinishedAt == DateTime.UtcNow.Date
            );
    }

    [Fact]
    public async Task It_Can_Update_A_Dispatched_Jobs_Retry_Count()
    {
        // Arrange.
        var jobDefinition = new DispatchedJobDefinition { Id = "abc" };
        _memoryStoreDataContainer.DispatchedJobs.Add(jobDefinition);

        await using var writer = _transactionManager.BeginTransaction();

        // Act.
        await writer.UpdateJobRetriesAsync(jobDefinition.Id, 25, CancellationToken.None);
        await writer.CommitAsync(CancellationToken.None);

        // Assert.
        _memoryStoreDataContainer.DispatchedJobs
            .Should()
            .ContainSingle(j => j.Id == jobDefinition.Id && j.Retries == 25);
    }

    [Fact]
    public async Task It_Can_Delete_A_Scheduled_Job()
    {
        // Arrange.
        var scheduledJob = new ScheduledJobDefinition { Name = "to-delete" };
        _memoryStoreDataContainer.ScheduledJobs.Add(scheduledJob.Id, scheduledJob);

        await using var writer = _transactionManager.BeginTransaction();

        // Act.
        await writer.DeleteScheduledJobAsync(scheduledJob.Id, CancellationToken.None);
        await writer.CommitAsync(CancellationToken.None);

        // Assert.
        _memoryStoreDataContainer.AssertScheduledJobDoesNotExist(scheduledJob.Id);
    }

    [Fact]
    public async Task It_Does_Not_Commit_The_Writes_If_A_Failure_Occurs()
    {
        // Arrange.
        var jobDefinition = new DispatchedJobDefinition { Status = JobStatus.Created };

        _memoryStoreDataContainer.DispatchedJobs.Add(jobDefinition);

        // Act.
        try
        {
            await using var writer = _transactionManager.BeginTransaction();

            await writer.UpdateJobRetriesAsync(jobDefinition.Id, 25, CancellationToken.None);
            await writer.UpdateJobStateAsync(
                jobDefinition.Id,
                JobStatus.Complete,
                DateTime.UtcNow,
                CancellationToken.None
            );

            throw new Exception();
        }
        catch (Exception)
        {
            // Ignore.
        }

        // Assert.
        _memoryStoreDataContainer.AssertJobHasRetryCount(jobDefinition.Id, 0);
        _memoryStoreDataContainer.AssertStatusForJobIs(jobDefinition.Id, JobStatus.Created);
    }

    [Fact]
    public async Task It_Commits_Incremental_Writes_Successfully()
    {
        // Arrange.
        var server = new ServerInstance
        {
            Hostname = "localhost",
            Key = StringGenerationUtils.GenerateUniqueRandomString()
        };

        var dispatchedJob = new DispatchedJobDefinition();
        var scheduledJob = new ScheduledJobDefinition { Name = "incremental" };

        // Act.
        await using var writer = _transactionManager.BeginTransaction();

        await writer.CreateServerAsync(server, CancellationToken.None);
        await writer.CreateServerHeartbeatAsync(server.Id, CancellationToken.None);
        await writer.CreateServerHeartbeatAsync(server.Id, CancellationToken.None);
        await writer.CreateServerHeartbeatAsync(server.Id, CancellationToken.None);
        await writer.CreateServerHeartbeatAsync(server.Id, CancellationToken.None);
        await writer.CreateWorkerAsync(
            new Worker { ServerInstanceId = server.Id },
            CancellationToken.None
        );
        await writer.CreateOrUpdateScheduledJobAsync(scheduledJob, CancellationToken.None);
        await writer.UpdateScheduledJobNextRunDateAsync(
            scheduledJob.Id,
            DateTime.Now.AddDays(-1).Date,
            CancellationToken.None
        );
        await writer.UpdateScheduledJobNextRunDateAsync(
            scheduledJob.Id,
            DateTime.Now.AddDays(1).Date,
            CancellationToken.None
        );
        await writer.CreateDispatchedJobAsync(dispatchedJob, CancellationToken.None);
        await writer.UpdateJobRetriesAsync(dispatchedJob.Id, 25, CancellationToken.None);
        await writer.UpdateJobStateAsync(
            dispatchedJob.Id,
            JobStatus.Complete,
            DateTime.UtcNow,
            CancellationToken.None
        );

        await writer.CommitAsync(CancellationToken.None);

        // Assert.
        _memoryStoreDataContainer.AssertHasRegisteredAServer();
        _memoryStoreDataContainer.AssertHeartbeatRegisteredForServer(server.Id, 4);
        _memoryStoreDataContainer.AssertHasRegisteredWorkersForServer(server.Id, 1);
        _memoryStoreDataContainer.AssertScheduledJobExists(scheduledJob.Id);
        _memoryStoreDataContainer.AssertNextRunDateForScheduledJobIsCloseTo(
            scheduledJob.Id,
            DateTime.Now.AddDays(1).Date
        );
        _memoryStoreDataContainer.AssertStatusForJobIs(dispatchedJob.Id, JobStatus.Complete);
        _memoryStoreDataContainer.AssertJobHasRetryCount(dispatchedJob.Id, 25);
    }
}
