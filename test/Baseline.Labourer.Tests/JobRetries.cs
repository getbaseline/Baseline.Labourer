using System;
using System.Threading.Tasks;
using Baseline.Labourer.Server;
using Baseline.Labourer.Tests.Configurations;
using Baseline.Labourer.Tests.Internal;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Tests;

public class JobRetries : BaseTest
{
    public JobRetries(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <summary>
    /// Tests that a job can be retried up to the globally configured amount of retries before the server retries
    /// no more and marks the job as catastrophically failed.
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task RetriesAJobUpToItsConfiguredAmountOfRetries_ThenMarksItAsACatastrophicFailure(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        await BootstrapAsync(
            queueProvider,
            storeProvider,
            serverConfiguration =>
            {
                serverConfiguration.DefaultRetryConfiguration = new RetryConfiguration(
                    2,
                    TimeSpan.Zero
                );
            }
        );

        // Act.
        await Client.DispatchJobAsync<RetriesAJobUpToItsConfiguredAmountOfRetries_ThenMarksItAsACatastrophicFailureJob>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                RetriesAJobUpToItsConfiguredAmountOfRetries_ThenMarksItAsACatastrophicFailureJob.Counter
                    .Should()
                    .Be(3);
            }
        );
        RetriesAJobUpToItsConfiguredAmountOfRetries_ThenMarksItAsACatastrophicFailureJob.Counter =
            0;
    }

    /// <summary>
    /// Tests that even if a job fails for X iterations, it can still complete successfully.
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task CompletesAJobEvenIfItFailsACoupleOfTimesFirst(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        await BootstrapAsync(
            queueProvider,
            storeProvider,
            serverConfigurer =>
            {
                serverConfigurer.DefaultRetryConfiguration = new RetryConfiguration(
                    2,
                    TimeSpan.Zero
                );
            }
        );

        // Act.
        await Client.DispatchJobAsync<CompletesAJobEvenIfItFailsACoupleOfTimesFirstJob>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                CompletesAJobEvenIfItFailsACoupleOfTimesFirstJob.Completed.Should().BeTrue();
            }
        );
        CompletesAJobEvenIfItFailsACoupleOfTimesFirstJob.Completed = false;
    }

    /// <summary>
    /// Verifies that individual retry configurations can be configured per job type.
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task CanChangeTheRetryConfigurationPerJob(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        await BootstrapAsync(
            queueProvider,
            storeProvider,
            serverConfigurer =>
            {
                serverConfigurer.DefaultRetryConfiguration = new RetryConfiguration(
                    0,
                    TimeSpan.FromHours(1)
                );
                serverConfigurer.JobRetryConfigurations.Add(
                    typeof(CanChangeTheRetryConfigurationPerJobJob),
                    new RetryConfiguration(1, TimeSpan.Zero)
                );
            }
        );

        // Act.
        await Client.DispatchJobAsync<CanChangeTheRetryConfigurationPerJobJob>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                CanChangeTheRetryConfigurationPerJobJob.Counter.Should().Be(2);
            }
        );
        CanChangeTheRetryConfigurationPerJobJob.Counter = 0;
    }

    #region Test Dependencies

    public class RetriesAJobUpToItsConfiguredAmountOfRetries_ThenMarksItAsACatastrophicFailureJob
        : IJob
    {
        public static int Counter;

        public ValueTask HandleAsync()
        {
            Counter++;
            throw new Exception();
        }
    }

    public class CompletesAJobEvenIfItFailsACoupleOfTimesFirstJob : IJob
    {
        private static int Counter;
        public static bool Completed;

        public ValueTask HandleAsync()
        {
            Counter++;

            if (Counter % 3 != 0)
            {
                throw new Exception();
            }

            Completed = true;
            return ValueTask.CompletedTask;
        }
    }

    public class CanChangeTheRetryConfigurationPerJobJob : IJob
    {
        public static int Counter;

        public ValueTask HandleAsync()
        {
            Counter++;
            throw new Exception();
        }
    }

    #endregion
}
