using System.Linq;
using System.Threading.Tasks;
using Baseline.Labourer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests.Workers;

public class ServerHeartbeatWorkerTests : ServerTest
{
    public ServerHeartbeatWorkerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    { }

    [Fact]
    public async Task It_Successfully_Registers_A_Server_Heartbeat()
    {
        // Arrange.
        var configuration = GenerateServerConfiguration();

        // Act.
#pragma warning disable CS4014
        Task.Run(async () => await new LabourerServer(configuration).RunServerAsync());
#pragma warning restore CS4014

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                var serverId = TestStoreDataContainer.Servers.First().Id;
                TestStoreDataContainer.AssertHeartbeatRegisteredForServer(serverId);
            }
        );
    }
}
