using System.Threading.Tasks;
using Baseline.Labourer.Tests.Scenarios.Configurations;
using Baseline.Labourer.Tests.Scenarios.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Tests.Scenarios;

public class JobMiddlewares : BaseTest
{
    public JobMiddlewares(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task RunsTheRelevantMiddlewareMethodsThroughoutTheExecutionStatusesOfAJob() { }

    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToStopContinuingExecution() { }

    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task DoesNotRunAnyFurtherMiddleware_WhenTheMiddlewareSaysToAbortOnFailure() { }
}
