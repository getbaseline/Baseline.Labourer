using System.Threading.Tasks;
using Baseline.Labourer.Tests.Scenarios.Configurations;
using Baseline.Labourer.Tests.Scenarios.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Tests.Scenarios;

public class JobRetries : BaseTest
{
    public JobRetries(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task RetriesAJobUpToItsConfiguredAmountOfRetries_ThenMarksItAsACatastrophicFailure() { }

    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task CompletesAJobEvenIfItFailsACoupleOfTimesFirst() { }

    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task CanChangeTheRetryConfigurationPerJob() { }
}
