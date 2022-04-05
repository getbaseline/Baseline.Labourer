using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Tests.Configurations;
using Baseline.Labourer.Tests.Internal;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Tests;

public class SchedulingJobs : BaseTest
{
    public SchedulingJobs(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <summary>
    /// Verifies that numerous scheduled jobs can be created, marked as requiring running, and subsequently executed.
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task ScheduledJobsCanBeCreatedAndGetExecuted(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        await BootstrapAsync(queueProvider, storeProvider);
        var nextHourDateTime = DateTime.UtcNow.AddHours(1);

        // Act.
        for (var i = 0; i < 10; i++)
        {
            await Client.CreateOrUpdateScheduledJobAsync<ScheduledJobCanBeCreatedAndGetExecutedJob>(
                i.ToString(),
                $"0 {nextHourDateTime.Hour} * * *"
            );
        }
        TestDateTimeProvider.SetUtcNow(nextHourDateTime.AddMinutes(1));

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                ScheduledJobCanBeCreatedAndGetExecutedJob.Counter.Should().Be(10);
            },
            100
        );
        ScheduledJobCanBeCreatedAndGetExecutedJob.Counter = 0;
    }

    /// <summary>
    /// Verifies that scheduled jobs with parameters can be created, marked as requiring running and subsequently
    /// executed.
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task ScheduledJobsCanBeCreatedAndGetExecutedWhenTheyHaveParameters(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        await BootstrapAsync(queueProvider, storeProvider);
        var nextHourDateTime = DateTime.UtcNow.AddHours(1);

        // Act.
        for (var i = 0; i < 10; i++)
        {
            await Client.CreateOrUpdateScheduledJobAsync<
                ScheduledJobCanBeCreatedAndGetExecutedWhenTheyHaveParametersParameters,
                ScheduledJobCanBeCreatedAndGetExecutedWhenTheyHaveParametersJob
            >(
                i.ToString(),
                $"0 {nextHourDateTime.Hour} * * *",
                new ScheduledJobCanBeCreatedAndGetExecutedWhenTheyHaveParametersParameters(
                    "Bob",
                    48
                )
            );
        }
        TestDateTimeProvider.SetUtcNow(nextHourDateTime.AddMinutes(1));

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                ScheduledJobCanBeCreatedAndGetExecutedWhenTheyHaveParametersJob.Counter
                    .Should()
                    .Be(10);
            },
            100
        );
        ScheduledJobCanBeCreatedAndGetExecutedWhenTheyHaveParametersJob.Counter = 0;
    }

    /// <summary>
    /// Ensures that a scheduled job can be repeated multiple times and isn't masquerading as a one off, dispatched
    /// job!
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task ScheduledJobsCanBeRanMultipleTimes(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        await BootstrapAsync(queueProvider, storeProvider);

        // Act.
        await Client.CreateOrUpdateScheduledJobAsync<ScheduledJobsCanBeRanMultipleTimesJob>(
            "run-multiple-times",
            "0 * * * *"
        );

        // Assert.
        TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddHours(1));
        await AssertionUtils.RetryAsync(
            () =>
            {
                ScheduledJobsCanBeRanMultipleTimesJob.Counter.Should().Be(1);
            }
        );

        TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddHours(2));
        await AssertionUtils.RetryAsync(
            () =>
            {
                ScheduledJobsCanBeRanMultipleTimesJob.Counter.Should().Be(2);
            }
        );

        TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddHours(3));
        await AssertionUtils.RetryAsync(
            () =>
            {
                ScheduledJobsCanBeRanMultipleTimesJob.Counter.Should().Be(3);
            }
        );
        ScheduledJobsCanBeRanMultipleTimesJob.Counter = 0;
    }

    /// <summary>
    /// Verifies that where a scheduled job has an existing lock (i.e. another processor has already picked the job
    /// up and is working on it) that no errors are thrown and the locked job is simply skipped from the processor's
    /// queue.
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task ScheduledJobsThatAreLockedAreIgnoredByOtherProcessors(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        await BootstrapAsync(queueProvider, storeProvider);

        // Act.
        await Client.CreateOrUpdateScheduledJobAsync<ScheduledJobsAreNotExecutedIfLockedJob>(
            "is-locked",
            $"* {DateTime.UtcNow.AddHours(1).Hour} * * *"
        );
        await StoreWrapper.Store.ResourceLocker.LockResourceAsync(
            "scheduled-job:is-locked",
            TimeSpan.FromHours(100)
        );
        TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddDays(2));

        // Assert.
        await AssertionUtils.EnsureAsync(
            () =>
            {
                ScheduledJobsAreNotExecutedIfLockedJob.Executed.Should().BeFalse();
            }
        );
    }

    /// <summary>
    /// Tests that scheduled jobs and their properties can be updated, and that update results in a change of the
    /// execution routine.
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task ScheduledJobsCanBeUpdated(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        await BootstrapAsync(queueProvider, storeProvider);
        var futureDateTime = DateTime.Now.AddYears(1);

        await Client.CreateOrUpdateScheduledJobAsync<
            ScheduledJobCanBeUpdatedParameters,
            ScheduledJobCanBeUpdatedJob
        >(
            "can-be-updated",
            $"0 {futureDateTime.Hour} {futureDateTime.Day} {futureDateTime.Month} {futureDateTime.DayOfWeek}",
            new ScheduledJobCanBeUpdatedParameters("before")
        );

        // Act.
        await Client.CreateOrUpdateScheduledJobAsync<
            ScheduledJobCanBeUpdatedParameters,
            ScheduledJobCanBeUpdatedJob
        >("can-be-updated", "* * * * *", new ScheduledJobCanBeUpdatedParameters("after"));
        TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddMinutes(3));

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                ScheduledJobCanBeUpdatedJob.State.Should().Be("after");
            }
        );
        ScheduledJobCanBeUpdatedJob.State = null;
    }

    /// <summary>
    /// Tests that scheduled jobs can be deleted and are never executed again.
    /// </summary>
    [Theory]
    [ClassData(typeof(RunOnAllProvidersConfiguration))]
    public async Task ScheduledJobsCanBeRemoved(
        QueueProvider queueProvider,
        StoreProvider storeProvider
    )
    {
        // Arrange.
        await BootstrapAsync(queueProvider, storeProvider);
        await Client.CreateOrUpdateScheduledJobAsync<ScheduledJobCanBeDeletedJob>(
            "can-be-deleted",
            "0 0 * * *"
        );

        // Act.
        await Client.DeleteScheduledJobAsync("can-be-deleted");
        TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddHours(5));

        // Assert.
        await AssertionUtils.EnsureAsync(
            () =>
            {
                ScheduledJobCanBeDeletedJob.Executed.Should().BeFalse();
            }
        );
    }

    #region Test Dependencies
    public class ScheduledJobCanBeCreatedAndGetExecutedJob : IJob
    {
        private readonly SemaphoreSlim _semaphore = new(1);
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

    public class ScheduledJobsCanBeRanMultipleTimesJob : IJob
    {
        public static int Counter;

        public ValueTask HandleAsync()
        {
            Counter++;

            return ValueTask.CompletedTask;
        }
    }

    public class ScheduledJobsAreNotExecutedIfLockedJob : IJob
    {
        public static bool Executed;

        public ValueTask HandleAsync()
        {
            Executed = true;
            return ValueTask.CompletedTask;
        }
    }

    public record ScheduledJobCanBeCreatedAndGetExecutedWhenTheyHaveParametersParameters(
        string Name,
        int Age
    );

    public class ScheduledJobCanBeCreatedAndGetExecutedWhenTheyHaveParametersJob
        : IJob<ScheduledJobCanBeCreatedAndGetExecutedWhenTheyHaveParametersParameters>
    {
        private readonly SemaphoreSlim _semaphore = new(1);
        public static int Counter;

        public async ValueTask HandleAsync(
            ScheduledJobCanBeCreatedAndGetExecutedWhenTheyHaveParametersParameters parameters
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

    public record ScheduledJobCanBeUpdatedParameters(string State);

    public class ScheduledJobCanBeUpdatedJob : IJob<ScheduledJobCanBeUpdatedParameters>
    {
        public static string? State;

        public ValueTask HandleAsync(ScheduledJobCanBeUpdatedParameters parameters)
        {
            State = parameters.State;

            return ValueTask.CompletedTask;
        }
    }

    public class ScheduledJobCanBeDeletedJob : IJob
    {
        public static bool Executed;

        public ValueTask HandleAsync()
        {
            Executed = true;
            return ValueTask.CompletedTask;
        }
    }

    #endregion
}
