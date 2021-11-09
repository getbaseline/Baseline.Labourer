using Baseline.Labourer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests.Workers;

public class ServerHeartbeatWorkerTests : ServerTest
{
    public ServerHeartbeatWorkerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task It_Successfully_Registers_A_Server_Heartbeat()
    {
        // Arrange.
        var context = await GenerateServerContextAsync();

        // Act.
        Task.Run(async () => await new ServerHeartbeatWorker.ServerHeartbeatWorker(context).RunAsync());

        // Assert.
        await AssertionUtils.RetryAsync(() => TestServerStore.AssertHeartbeatRegisteredForServer(ServerId));
    }
}
