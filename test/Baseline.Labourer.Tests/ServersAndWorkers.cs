using System.Threading.Tasks;
using Baseline.Labourer.Tests.Configurations;
using Baseline.Labourer.Tests.Internal;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Tests;

public class ServersAndWorkers : BaseTest
{
    public ServersAndWorkers(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <summary>
    /// Tests that on boot of the server a server instance and all workers are registered within the relevant store.
    /// TODO: Utilise reader methods to test this from an actual users perspective. At the minute - nobody cares whether these are registered or not!
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task RegistersServersAndWorkersOnBootOfServer(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange/Act.
        await BootstrapAsync(queueProvider, storeProvider);

        // Assert.
        await AssertionUtils.RetryAsync(
            async () =>
            {
                var servers = await StoreWrapper.GetRegisteredServersAsync();
                servers.Count.Should().Be(1);

                var workers = await StoreWrapper.GetRegisteredWorkersAsync();
                workers.Count.Should().Be(20);
            }
        );
    }
}
