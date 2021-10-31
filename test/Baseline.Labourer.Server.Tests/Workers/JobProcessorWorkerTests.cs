using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Server.Workers;
using Baseline.Labourer.Tests;
using FluentAssertions;
using Xunit;

namespace Baseline.Labourer.Server.Tests.Workers
{
    public class JobProcessorWorkerTests : ServerTest, IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        
        public JobProcessorWorkerTests() : base()
        {
            Task.Run(
                async () => await new JobProcessorWorker(
                    new BaselineServerConfiguration { ShutdownTokenSource = _cancellationTokenSource }, 
                    TestJobStore, 
                    TestQueue
                ).RunAsync()
            );
        }
        
        public class SimpleQueuedJobParams {}
        public class SimpleQueuedJob : IJob<SimpleQueuedJobParams>
        {
            internal static bool Handled = false;
            
            public Task HandleAsync(SimpleQueuedJobParams parameters, CancellationToken cancellationToken)
            {
                Handled = true;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task It_Processes_A_Simple_Queued_Job()
        {
            // Act.
            await Client.DispatchJobAsync<SimpleQueuedJobParams, SimpleQueuedJob>(
                new SimpleQueuedJobParams()
            );
            
            // Assert.
            await AssertionUtils.Retry(() => SimpleQueuedJob.Handled.Should().BeTrue());
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
                new SimpleQueuedJobWithParamsParams {Count = 100}
            );
            
            // Assert.
            await AssertionUtils.Retry(() => SimpleQueuedJobWithParams.Count.Should().Be(100));
        }
        
        public class LateJobParams {}

        public class LateJob : IJob<LateJobParams>
        {
            public static bool Handled = false;
            
            public Task HandleAsync(LateJobParams parameters, CancellationToken cancellationToken)
            {
                Handled = true;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task It_Processes_A_Job_That_Is_Added_At_A_Later_Point()
        {
            // Arrange.
            await Task.Delay(3500);
            
            // Act.
            await Client.DispatchJobAsync<LateJobParams, LateJob>(
                new LateJobParams()
            );
            
            // Assert.
            await AssertionUtils.Retry(() => LateJob.Handled.Should().BeTrue());
        }
        
        public class MarkedAsInProgressJobParams {}

        public class MarkedAsInProgressJob : IJob<MarkedAsInProgressJobParams>
        {
            public async Task HandleAsync(MarkedAsInProgressJobParams parameters, CancellationToken cancellationToken)
            {
                await Task.Delay(2500, cancellationToken);
            }
        }

        [Fact]
        public async Task It_Marks_A_Job_As_In_Progress_And_Then_Complete()
        {
            // Act.
            var jobId = await Client.DispatchJobAsync<MarkedAsInProgressJobParams, MarkedAsInProgressJob>(
                new MarkedAsInProgressJobParams()
            );
            
            // Assert.
            await AssertionUtils.Retry(() => TestJobStore.AssertStatusForJobIs(jobId, JobStatus.InProgress));
            await AssertionUtils.Retry(() =>
            {
                TestJobStore.AssertStatusForJobIs(jobId, JobStatus.Complete);
                TestJobStore.AssertJobHasFinishedAtValueWithin5SecondsOf(jobId, DateTime.UtcNow);
            });
        }
        
        // Tests for multiple workers (need to add some sort of worker tracking to the job).
        // Tests for multiple jobs through workers being completed.

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}