using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests.Workers.JobProcessorWorkerTests;

public class JobLockingTests : ServerTest
{
    public JobLockingTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [Fact]
    public async Task It_Fails_To_Execute_A_Job_If_The_Job_Is_Locked()
    {
        // Arrange.
        var jobId = await Client.DispatchJobAsync<BasicJob>();
        TestStoreDataContainer.Locks.Add(
            jobId,
            new List<MemoryLock> { new MemoryLock { Until = DateTime.UtcNow.AddHours(1) } }
        );

        // Act.
#pragma warning disable CS4014
        Task.Run(
#pragma warning restore CS4014
            async () => await new LabourerServer(GenerateServerConfiguration()).RunServerAsync()
        );
        await Task.Delay(1000);

        // Assert.
        TestStoreDataContainer.AssertStatusForJobIs(jobId, JobStatus.Created);
    }

    [Fact]
    public async Task It_Eventually_Executes_A_Job_If_The_Lock_Expires()
    {
        // Arrange.
        var jobId = await Client.DispatchJobAsync<BasicSuccessfulJob>();
        TestStoreDataContainer.Locks.Add(
            jobId,
            new List<MemoryLock> { new MemoryLock { Until = DateTime.UtcNow.AddHours(1) } }
        );

        // Act.
#pragma warning disable CS4014
        Task.Run(
#pragma warning restore CS4014
            async () => await new LabourerServer(GenerateServerConfiguration()).RunServerAsync()
        );
        await Task.Delay(1000);
        TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddHours(2));
        TestMemoryQueue.MakeAllMessagesVisible();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                TestStoreDataContainer.AssertStatusForJobIs(jobId, JobStatus.Complete);
            }
        );
    }
}
