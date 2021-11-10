using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests.Workers.JobProcessorWorkerTests
{
    public class ServerAndWorkerRegistrationTests : ServerTest, IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public ServerAndWorkerRegistrationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task It_Creates_And_Registers_The_Workers_Specified_In_The_Server_Context()
        {
            // Arrange.
            Task.Run(async () => await new JobProcessorWorker.JobProcessorWorker(await GenerateServerContextAsync(10)).RunAsync());

            // Assert.
            await AssertionUtils.RetryAsync(() => TestStore.AssertHasRegisteredWorkersForServer(ServerId, 10), 50);
        }
    }
}