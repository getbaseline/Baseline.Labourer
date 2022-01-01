using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Internal.Utils;
using Baseline.Labourer.Tests;
using FluentAssertions;
using Xunit;

namespace Baseline.Labourer.Store.Memory.Tests
{
    public class MemoryTransactionStoreWriterTests
    {
        private readonly TestMemoryStore _memoryStore = new TestMemoryStore();
        private readonly MemoryStoreWriterTransactionManager _transactionManager;

        public MemoryTransactionStoreWriterTests()
        {
            _transactionManager = new MemoryStoreWriterTransactionManager(_memoryStore);
        }
        
        [Fact]
        public async Task It_Can_Create_A_Server()
        {
            // Arrange.
            await using var writer = _transactionManager.BeginTransaction();
            
            // Act.
            await writer.CreateServerAsync(new ServerInstance {Hostname = "foo", Key = "bar"}, CancellationToken.None);
            await writer.CommitAsync(CancellationToken.None);

            // Assert.
            _memoryStore.Servers.Should().ContainSingle(s => s.Id == "foo-bar");
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
            _memoryStore.ServerHeartbeats["abc"].Should().HaveCount(1);
        }

        [Fact]
        public async Task It_Can_Create_A_Worker()
        {
            // Arrange.
            await using var writer = _transactionManager.BeginTransaction();
            
            // Act.
            await writer.CreateWorkerAsync(new Worker { Id = "foo", ServerInstanceId = "foo" }, CancellationToken.None);
            await writer.CommitAsync(CancellationToken.None);

            // Assert.
            _memoryStore.ServerWorkers["foo"].Should().ContainSingle(w => w.Id == "foo");
        }

        [Fact]
        public async Task It_Can_Create_A_Scheduled_Job()
        {
            // Arrange.
            await using var writer = _transactionManager.BeginTransaction();
            
            // Act.
            await writer.CreateOrUpdateScheduledJobDefinitionAsync(
                new ScheduledJobDefinition { Name = "scheduled-job", CronExpression = "abc" }, 
                CancellationToken.None
            );
            await writer.CommitAsync(CancellationToken.None);

            // Assert.
            _memoryStore.ScheduledJobs["scheduled-job:scheduled-job"].CronExpression.Should().Be("abc");
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
            _memoryStore.ScheduledJobs.Add(scheduledJob.Id, scheduledJob);
            
            await using var writer = _transactionManager.BeginTransaction();
            
            // Act.
            await writer.UpdateScheduledJobNextRunDateAsync(
                scheduledJob.Id,
                DateTime.UtcNow.Date.AddDays(7),
                CancellationToken.None
            );
            await writer.CommitAsync(CancellationToken.None);

            // Assert.
            _memoryStore.ScheduledJobs[scheduledJob.Id].NextRunDate.Should().Be(DateTime.UtcNow.Date.AddDays(7));
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
            _memoryStore.ScheduledJobs.Add(scheduledJob.Id, scheduledJob);
            
            await using var writer = _transactionManager.BeginTransaction();
            
            // Act.
            await writer.UpdateScheduledJobLastRunDateAsync(
                scheduledJob.Id,
                DateTime.UtcNow.Date.AddDays(7),
                CancellationToken.None
            );
            await writer.CommitAsync(CancellationToken.None);

            // Assert.
            _memoryStore.ScheduledJobs[scheduledJob.Id].LastRunDate.Should().Be(DateTime.UtcNow.Date.AddDays(7));
        }

        [Fact]
        public async Task It_Can_Create_A_Dispatched_Job()
        {
            // Arrange.
            await using var writer = _transactionManager.BeginTransaction();
            
            // Act.
            await writer.CreateDispatchedJobDefinitionAsync(
                new DispatchedJobDefinition {Id = "bar"}, 
                CancellationToken.None
            );
            await writer.CommitAsync(CancellationToken.None);

            // Assert.
            _memoryStore.DispatchedJobs.Should().ContainSingle(j => j.Id == "bar");
        }

        [Fact]
        public async Task It_Can_Update_A_Dispatched_Jobs_Status()
        {
            // Arrange.
            var jobDefinition = new DispatchedJobDefinition {Id = "abc"};
            _memoryStore.DispatchedJobs.Add(jobDefinition);
            
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
            _memoryStore.DispatchedJobs.Should().ContainSingle(
                j => j.Id == jobDefinition.Id &&
                     j.Status == JobStatus.Complete &&
                     j.FinishedAt == DateTime.UtcNow.Date
            );
        }

        [Fact]
        public async Task It_Can_Update_A_Dispatched_Jobs_Retry_Count()
        {
            // Arrange.
            var jobDefinition = new DispatchedJobDefinition {Id = "abc"};
            _memoryStore.DispatchedJobs.Add(jobDefinition);
            
            await using var writer = _transactionManager.BeginTransaction();
            
            // Act.
            await writer.UpdateJobRetriesAsync(
                jobDefinition.Id, 
                25,
                CancellationToken.None
            );
            await writer.CommitAsync(CancellationToken.None);

            // Assert.
            _memoryStore.DispatchedJobs.Should().ContainSingle(j => j.Id == jobDefinition.Id && j.Retries == 25);
        }

        [Fact]
        public async Task It_Does_Not_Commit_The_Writes_If_A_Failure_Occurs()
        {
            // Arrange.
            var jobDefinition = new DispatchedJobDefinition
            {
                Status = JobStatus.Created
            };

            _memoryStore.DispatchedJobs.Add(jobDefinition);

            // Act.
            try
            {
                await using var writer = _transactionManager.BeginTransaction();

                await writer.UpdateJobRetriesAsync(jobDefinition.Id, 25, CancellationToken.None);
                await writer.UpdateJobStateAsync(jobDefinition.Id, JobStatus.Complete, DateTime.UtcNow, CancellationToken.None);

                throw new Exception();
            }
            catch (Exception)
            {
            }

            // Assert.
            _memoryStore.AssertJobHasRetryCount(jobDefinition.Id, 0);
            _memoryStore.AssertStatusForJobIs(jobDefinition.Id, JobStatus.Created);
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
            await writer.CreateWorkerAsync(new Worker { ServerInstanceId = server.Id }, CancellationToken.None);
            await writer.CreateOrUpdateScheduledJobDefinitionAsync(scheduledJob, CancellationToken.None);
            await writer.UpdateScheduledJobNextRunDateAsync(scheduledJob.Id, DateTime.Now.AddDays(-1).Date, CancellationToken.None);
            await writer.UpdateScheduledJobNextRunDateAsync(scheduledJob.Id, DateTime.Now.AddDays(1).Date, CancellationToken.None);
            await writer.CreateDispatchedJobDefinitionAsync(dispatchedJob, CancellationToken.None);
            await writer.UpdateJobRetriesAsync(dispatchedJob.Id, 25, CancellationToken.None);
            await writer.UpdateJobStateAsync(dispatchedJob.Id, JobStatus.Complete, DateTime.UtcNow, CancellationToken.None);

            await writer.CommitAsync(CancellationToken.None);

            // Assert.
            _memoryStore.AssertHasRegisteredAServer();
            _memoryStore.AssertHeartbeatRegisteredForServer(server.Id, 4);
            _memoryStore.AssertHasRegisteredWorkersForServer(server.Id, 1);
            _memoryStore.AssertScheduledJobCreated(scheduledJob.Id);
            _memoryStore.AssertNextRunDateForScheduledJobIsCloseTo(scheduledJob.Id, DateTime.Now.AddDays(1).Date);
            _memoryStore.AssertStatusForJobIs(dispatchedJob.Id, JobStatus.Complete);
            _memoryStore.AssertJobHasRetryCount(dispatchedJob.Id, 25);
        }
    }
}