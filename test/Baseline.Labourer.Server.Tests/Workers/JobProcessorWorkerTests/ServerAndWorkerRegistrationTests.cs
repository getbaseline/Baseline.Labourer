using System.Linq;
using System.Threading.Tasks;
using Baseline.Labourer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests.Workers.JobProcessorWorkerTests;

public class ServerAndWorkerRegistrationTests : ServerTest
{
    public ServerAndWorkerRegistrationTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper) { }

    [Fact]
    public async Task It_Creates_And_Registers_The_Workers_Specified_In_The_Server_Context()
    {
        // Arrange.
#pragma warning disable CS4014
        Task.Run(
            async () =>
                await new LabourerServer(
#pragma warning restore CS4014
                    GenerateServerConfiguration(s => s.JobProcessingWorkersToRun = 10)
                ).RunServerAsync()
        );

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                var serverId = TestStoreDataContainer.Servers.First().Id;
                TestStoreDataContainer.AssertHasRegisteredWorkersForServer(serverId, 10);
            },
            50
        );
    }
}
