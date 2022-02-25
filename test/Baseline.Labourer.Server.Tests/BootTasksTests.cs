using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Tests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests
{
    public class BootTasksTests : ServerTest
    {
        private class BootMemoryQueue : IQueue
        {
            public bool Bootstrapped { get; private set; }
            
            public ValueTask BootstrapAsync()
            {
                Bootstrapped = true;
                return new ValueTask();
            }

            public Task EnqueueAsync<T>(T messageToQueue, TimeSpan? visibilityDelay, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task<QueuedJob?> DequeueAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public Task DeleteMessageAsync(string messageId, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
        
        public BootTasksTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
        
        [Fact]
        public async Task It_Bootstraps_The_Store()
        {
            // Act.
#pragma warning disable CS4014
            Task.Run(async () => await new LabourerServer(GenerateServerConfiguration()).RunServerAsync());
#pragma warning restore CS4014

            // Assert.
            await AssertionUtils.RetryAsync(() =>
            {
                TestMemoryStore.Bootstrapped.Should().BeTrue();
            });
        }

        [Fact]
        public async Task It_Bootstraps_The_Queue()
        {
            // Arrange.
            var queue = new BootMemoryQueue();
            
            // Act.
#pragma warning disable CS4014
            Task.Run(async () => await new LabourerServer(GenerateServerConfiguration(c => c.Queue = queue)).RunServerAsync());
#pragma warning restore CS4014
            
            // Act.
            await AssertionUtils.RetryAsync(() =>
            {
                queue.Bootstrapped.Should().BeTrue();
            });
        }
    }
}