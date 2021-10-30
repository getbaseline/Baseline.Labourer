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
                    MemoryJobStore, 
                    MemoryQueue
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

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}