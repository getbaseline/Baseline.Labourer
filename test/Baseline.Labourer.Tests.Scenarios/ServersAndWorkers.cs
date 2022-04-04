using System.Threading.Tasks;
using Baseline.Labourer.Tests.Scenarios.Configurations;
using Baseline.Labourer.Tests.Scenarios.Internal;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Tests.Scenarios;

public class ServersAndWorkers : BaseTest
{
    public ServersAndWorkers(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

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
