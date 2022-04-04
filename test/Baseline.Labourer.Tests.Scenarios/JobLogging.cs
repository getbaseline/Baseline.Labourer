using System.Threading.Tasks;
using Baseline.Labourer.Tests.Scenarios.Configurations;
using Baseline.Labourer.Tests.Scenarios.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Tests.Scenarios;

public class JobLogging : BaseTest
{
    public JobLogging(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task LogsRelevantMessagesToTheProvidedLoggingFactory() { }

    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task LogsEventsFromExecutionOfJobInTheStore() { }
}
