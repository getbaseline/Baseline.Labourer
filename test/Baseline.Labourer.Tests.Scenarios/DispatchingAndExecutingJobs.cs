using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Tests.Scenarios.Configurations;
using Baseline.Labourer.Tests.Scenarios.Internal;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Tests.Scenarios;

public class DispatchingAndExecutingJobs : BaseTest
{
    public DispatchingAndExecutingJobs(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    { }

    /// <summary>
    /// Dispatches hundreds of jobs and verifies that they are all executed.
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task HundredsOfJobsCanBeDispatchedAndExecuted(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        await BootstrapAsync(queueProvider, storeProvider);

        // Act.
        for (var i = 0; i < 500; i++)
        {
            await Client.DispatchJobAsync<HundredsOfJobsCanBeDispatchedAndExecutedJob>();
        }

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                HundredsOfJobsCanBeDispatchedAndExecutedJob.Counter.Should().Be(500);
            },
            100
        );
        HundredsOfJobsCanBeDispatchedAndExecutedJob.Counter = 0;
    }

    /// <summary>
    /// Dispatches hundreds of jobs that have parameters and verifies that they are all executed.
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task HundredsOfJobsWithParametersCanBeDispatchedAndExecuted(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        await BootstrapAsync(queueProvider, storeProvider);

        // Act.
        for (var i = 0; i < 500; i++)
        {
            await Client.DispatchJobAsync<
                HundredsOfJobsWithParametersCanBeDispatchedAndExecutedParameters,
                HundredsOfJobsWithParametersCanBeDispatchedAndExecutedJob
            >(new HundredsOfJobsWithParametersCanBeDispatchedAndExecutedParameters("Bob", 50));
        }

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                HundredsOfJobsWithParametersCanBeDispatchedAndExecutedJob.Counter.Should().Be(500);
            },
            100
        );
        HundredsOfJobsWithParametersCanBeDispatchedAndExecutedJob.Counter = 0;
    }

    #region Test Classes
    public class HundredsOfJobsCanBeDispatchedAndExecutedJob : IJob
    {
        private static SemaphoreSlim _semaphore = new(1);
        public static int Counter;

        public async ValueTask HandleAsync()
        {
            try
            {
                await _semaphore.WaitAsync();

                Counter++;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public record HundredsOfJobsWithParametersCanBeDispatchedAndExecutedParameters(
        string Name,
        int Age
    );

    public class HundredsOfJobsWithParametersCanBeDispatchedAndExecutedJob
        : IJob<HundredsOfJobsWithParametersCanBeDispatchedAndExecutedParameters>
    {
        private static SemaphoreSlim _semaphore = new(1);
        public static int Counter;

        public async ValueTask HandleAsync(
            HundredsOfJobsWithParametersCanBeDispatchedAndExecutedParameters parameters
        )
        {
            try
            {
                await _semaphore.WaitAsync();

                Counter++;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
    #endregion
}
