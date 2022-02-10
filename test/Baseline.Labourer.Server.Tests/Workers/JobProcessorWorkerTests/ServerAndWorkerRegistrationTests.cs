using System.Threading.Tasks;
using Baseline.Labourer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests.Workers.JobProcessorWorkerTests
{
    public class ServerAndWorkerRegistrationTests : ServerTest
    {
        public ServerAndWorkerRegistrationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task It_Creates_And_Registers_The_Workers_Specified_In_The_Server_Context()
        {
            // Arrange.
            Task.Run(async () => await new LabourerServer(
                GenerateServerConfiguration(s => s.JobProcessingWorkersToRun = 10)
            ).RunServerAsync());

            // Assert.
            await AssertionUtils.RetryAsync(() => TestBackingStore.AssertHasRegisteredWorkersForServer(ServerId, 10), 50);
        }
    }
}