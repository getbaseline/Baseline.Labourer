using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Tests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests.Workers.JobProcessorWorkerTests
{
    public class SimpleJobProcessingTests : ServerTest
    {
        public SimpleJobProcessingTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Task.Run(
                async () => await new LabourerServer(GenerateServerConfiguration()).RunServerAsync()
            );
        }

        public class SimpleQueuedJob : IJob
        {
            internal static bool Handled = false;

            public ValueTask HandleAsync(CancellationToken cancellationToken)
            {
                Handled = true;
                return new ValueTask();
            }
        }

        [Fact]
        public async Task It_Processes_A_Simple_Queued_Job()
        {
            // Act.
            await Client.DispatchJobAsync<SimpleQueuedJob>();

            // Assert.
            await AssertionUtils.RetryAsync(() => SimpleQueuedJob.Handled.Should().BeTrue());
        }

        public class SimpleQueuedJobWithParamsParams
        {
            public int Count { get; set; } = 10;
        }

        public class SimpleQueuedJobWithParams : IJob<SimpleQueuedJobWithParamsParams>
        {
            public static int Count = 0;

            public Task HandleAsync(SimpleQueuedJobWithParamsParams parameters, CancellationToken cancellationToken)
            {
                Count = parameters.Count;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task It_Processes_A_Simple_Queued_Job_That_Has_Parameters()
        {
            // Act.
            await Client.DispatchJobAsync<SimpleQueuedJobWithParamsParams, SimpleQueuedJobWithParams>(
                new SimpleQueuedJobWithParamsParams { Count = 100 }
            );

            // Assert.
            await AssertionUtils.RetryAsync(() => SimpleQueuedJobWithParams.Count.Should().Be(100));

            var serverId = TestBackingStore.AssertHasRegisteredAServer();
            TestBackingStore.AssertHasRegisteredWorkersForServer(serverId);
        }

        public class LateJob : IJob
        {
            public static bool Handled = false;

            public ValueTask HandleAsync(CancellationToken cancellationToken)
            {
                Handled = true;
                return new ValueTask();
            }
        }

        [Fact]
        public async Task It_Processes_A_Job_That_Is_Added_At_A_Later_Point()
        {
            // Arrange.
            await Task.Delay(3500);

            // Act.
            await Client.DispatchJobAsync<LateJob>();

            // Assert.
            await AssertionUtils.RetryAsync(() => LateJob.Handled.Should().BeTrue());
        }

        public class MarkedAsInProgressJob : IJob
        {
            public async ValueTask HandleAsync(CancellationToken cancellationToken)
            {
                await Task.Delay(2500, cancellationToken);
            }
        }

        [Fact]
        public async Task It_Marks_A_Job_As_In_Progress_And_Then_Complete()
        {
            // Act.
            var jobId = await Client.DispatchJobAsync<MarkedAsInProgressJob>();

            // Assert.
            await AssertionUtils.RetryAsync(() => TestBackingStore.AssertStatusForJobIs(jobId, JobStatus.InProgress));
            await AssertionUtils.RetryAsync(() =>
            {
                TestBackingStore.AssertStatusForJobIs(jobId, JobStatus.Complete);
                TestBackingStore.AssertJobHasFinishedAtValueWithin5SecondsOf(jobId, DateTime.UtcNow);
            }, 25, 500);
        }

        public class MultipleJobsJob : IJob
        {
            public ValueTask HandleAsync(CancellationToken cancellationToken)
            {
                return new ValueTask();
            }
        }

        [Fact]
        public async Task It_Completes_Multiple_Jobs_Successfully()
        {
            // Act.
            var jobIds = await Task.WhenAll(
                Enumerable.Range(0, 100)
                    .Select(async _ => await Client.DispatchJobAsync<MultipleJobsJob>())
            );

            // Assert.
            await AssertionUtils.RetryAsync(() =>
            {
                foreach (var jobId in jobIds)
                {
                    TestBackingStore.AssertStatusForJobIs(jobId, JobStatus.Complete);
                    TestBackingStore.AssertJobHasFinishedAtValueWithin5SecondsOf(jobId, DateTime.UtcNow);
                }
            });
        }
    }
}