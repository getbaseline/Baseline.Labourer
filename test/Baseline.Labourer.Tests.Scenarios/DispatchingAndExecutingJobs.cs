using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Tests.Scenarios.Internal;
using Baseline.Labourer.Tests.Scenarios.Setup;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit;

namespace Baseline.Labourer.Tests.Scenarios;

public class DispatchingAndExecutingJobs : BaseTest
{
    public class HundredsOfJobsCanBeDispatchedAndExecutedJob : IJob
    {
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1);
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

    [Theory]
    [InlineData(QueueProvider.Memory, StoreProvider.Memory)]
    [InlineData(QueueProvider.SQLite, StoreProvider.SQLite)]
    public async Task HundredsOfJobsCanBeDispatchedAndExecuted(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        await BootstrapAsync(queueProvider, storeProvider);

        // Act.
        for (int i = 0; i < 1000; i++)
        {
            await Client.DispatchJobAsync<HundredsOfJobsCanBeDispatchedAndExecutedJob>();
        }

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                HundredsOfJobsCanBeDispatchedAndExecutedJob.Counter.Should().Be(1000);
            }
        );
    }
}
