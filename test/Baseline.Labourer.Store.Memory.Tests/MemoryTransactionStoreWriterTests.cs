using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Utils;
using Baseline.Labourer.Tests;
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
            var scheduledJob = new ScheduledJobDefinition();

            // Act.
            await using var writer = _transactionManager.BeginTransaction();

            await writer.CreateServerAsync(server, CancellationToken.None);
            await writer.CreateServerHeartbeatAsync(server.Id, CancellationToken.None);
            await writer.CreateServerHeartbeatAsync(server.Id, CancellationToken.None);
            await writer.CreateServerHeartbeatAsync(server.Id, CancellationToken.None);
            await writer.CreateServerHeartbeatAsync(server.Id, CancellationToken.None);
            await writer.CreateWorkerAsync(new Worker { ServerInstanceId = server.Id }, CancellationToken.None);
            await writer.CreateScheduledJobDefinitionAsync(scheduledJob, CancellationToken.None);
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