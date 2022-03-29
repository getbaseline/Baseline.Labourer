using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests.Workers.JobProcessorWorkerTests;

public class JobRetryTests : ServerTest
{
    public JobRetryTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    public class CatastrophicErrorJob : IJob
    {
        public ValueTask HandleAsync()
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task It_Retries_A_Job_A_Maximum_Of_Three_Times_Before_Marking_The_Job_As_A_Catastrophic_Failure()
    {
        // Arrange.
        RunWorker();

        // Act.
        var jobId = await Client.DispatchJobAsync<CatastrophicErrorJob>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                TestStoreDataContainer.AssertJobHasRetryCount(jobId, 3);
                TestStoreDataContainer.AssertStatusForJobIs(
                    jobId,
                    JobStatus.FailedExceededMaximumRetries
                );
            }
        );
    }

    public class FailedJobThatCompletes : IJob
    {
        private static int _executions;

        public ValueTask HandleAsync()
        {
            _executions++;

            if (_executions != 3) // Pass on the third retry.
            {
                throw new NotImplementedException();
            }

            return new ValueTask();
        }
    }

    [Fact]
    public async Task It_Completes_A_Job_Even_If_It_Fails_A_Couple_Of_Times()
    {
        // Arrange.
        RunWorker();

        // Act.
        var jobId = await Client.DispatchJobAsync<FailedJobThatCompletes>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                TestStoreDataContainer.AssertJobHasRetryCount(jobId, 2);
                TestStoreDataContainer.AssertStatusForJobIs(jobId, JobStatus.Complete);
            }
        );
    }

    [Fact]
    public async Task The_Default_Retry_Amount_Can_Be_Changed()
    {
        // Arrange.
        RunWorker(new RetryConfiguration(10, TimeSpan.Zero));

        // Act.
        var jobId = await Client.DispatchJobAsync<CatastrophicErrorJob>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                TestStoreDataContainer.AssertJobHasRetryCount(jobId, 10);
                TestStoreDataContainer.AssertStatusForJobIs(
                    jobId,
                    JobStatus.FailedExceededMaximumRetries
                );
            }
        );
    }

    public class JobWithChangedRetryAmountThatCatastrophicallyErrors : IJob
    {
        public ValueTask HandleAsync()
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task The_Default_Retry_Amount_Can_Be_Changed_Per_Job()
    {
        // Arrange.
        RunWorker(
            new RetryConfiguration(5, TimeSpan.Zero),
            new Dictionary<Type, RetryConfiguration>
            {
                {
                    typeof(JobWithChangedRetryAmountThatCatastrophicallyErrors),
                    new RetryConfiguration(2, TimeSpan.Zero)
                }
            }
        );

        // Act.
        var standardJobId = await Client.DispatchJobAsync<CatastrophicErrorJob>();
        var changedJobId =
            await Client.DispatchJobAsync<JobWithChangedRetryAmountThatCatastrophicallyErrors>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                TestStoreDataContainer.AssertJobHasRetryCount(standardJobId, 5);
                TestStoreDataContainer.AssertStatusForJobIs(
                    standardJobId,
                    JobStatus.FailedExceededMaximumRetries
                );
                TestStoreDataContainer.AssertJobHasRetryCount(changedJobId, 2);
                TestStoreDataContainer.AssertStatusForJobIs(
                    changedJobId,
                    JobStatus.FailedExceededMaximumRetries
                );
            }
        );
    }

    [Fact]
    public async Task The_Default_Retry_Timeout_Can_Be_Changed_For_All_Jobs()
    {
        // Arrange.
        RunWorker(new RetryConfiguration(5, TimeSpan.FromSeconds(10)));

        // Act.
        var jobId = await Client.DispatchJobAsync<CatastrophicErrorJob>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                TestMemoryQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(
                    jobId,
                    0,
                    TimeSpan.Zero
                );
            }
        );

        TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddSeconds(11));

        await AssertionUtils.RetryAsync(
            () =>
            {
                TestStoreDataContainer.AssertStatusForJobIs(jobId, JobStatus.Failed);
                TestMemoryQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(
                    jobId,
                    1,
                    TimeSpan.FromSeconds(10)
                );
            }
        );

        TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddSeconds(22));

        await AssertionUtils.RetryAsync(
            () =>
            {
                TestStoreDataContainer.AssertStatusForJobIs(jobId, JobStatus.Failed);
                TestMemoryQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(
                    jobId,
                    2,
                    TimeSpan.FromSeconds(10)
                );
            }
        );

        TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddSeconds(33));

        await AssertionUtils.RetryAsync(
            () =>
            {
                TestStoreDataContainer.AssertStatusForJobIs(jobId, JobStatus.Failed);
                TestMemoryQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(
                    jobId,
                    3,
                    TimeSpan.FromSeconds(10)
                );
            }
        );

        TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddSeconds(44));

        await AssertionUtils.RetryAsync(
            () =>
            {
                TestStoreDataContainer.AssertStatusForJobIs(jobId, JobStatus.Failed);
                TestMemoryQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(
                    jobId,
                    4,
                    TimeSpan.FromSeconds(10)
                );
            }
        );

        TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddSeconds(55));

        await AssertionUtils.RetryAsync(
            () =>
            {
                TestStoreDataContainer.AssertStatusForJobIs(
                    jobId,
                    JobStatus.FailedExceededMaximumRetries
                );
                TestMemoryQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(
                    jobId,
                    5,
                    TimeSpan.FromSeconds(10)
                );
            }
        );
    }

    [Fact]
    public async Task The_Retry_Timeout_Can_Be_Changed_Per_Job()
    {
        // Arrange.
        RunWorker(
            new RetryConfiguration(5, TimeSpan.Zero),
            new Dictionary<Type, RetryConfiguration>
            {
                {
                    typeof(JobWithChangedRetryAmountThatCatastrophicallyErrors),
                    new RetryConfiguration(1, TimeSpan.FromSeconds(60))
                }
            }
        );

        // Act.
        var standardJobId = await Client.DispatchJobAsync<CatastrophicErrorJob>();
        var changedJobId =
            await Client.DispatchJobAsync<JobWithChangedRetryAmountThatCatastrophicallyErrors>();

        // Assert.
        await AssertionUtils.RetryAsync(
            () =>
            {
                TestStoreDataContainer.AssertJobHasRetryCount(changedJobId, 1);
                TestStoreDataContainer.AssertStatusForJobIs(changedJobId, JobStatus.Failed);

                // Verify the standard job with the no delay has been completed (and failed).
                TestStoreDataContainer.AssertStatusForJobIs(
                    standardJobId,
                    JobStatus.FailedExceededMaximumRetries
                );
                TestMemoryQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(
                    standardJobId,
                    5,
                    TimeSpan.Zero
                );
            }
        );

        TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddSeconds(70)); // Mimic the timeout actually occurring.

        await AssertionUtils.RetryAsync(
            () =>
            {
                // Verify the long delayed job has been completed (and failed).
                TestStoreDataContainer.AssertStatusForJobIs(
                    changedJobId,
                    JobStatus.FailedExceededMaximumRetries
                );
                TestMemoryQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(
                    changedJobId,
                    1,
                    TimeSpan.FromSeconds(60)
                );
            }
        );
    }

    private void RunWorker(
        RetryConfiguration? defaultRetryConfiguration = null,
        Dictionary<Type, RetryConfiguration>? jobRetryConfigurations = null
    )
    {
        Task.Run(
            async () =>
                await new LabourerServer(
                    GenerateServerConfiguration(
                        c =>
                        {
                            c.DefaultRetryConfiguration =
                                defaultRetryConfiguration
                                ?? new RetryConfiguration(3, TimeSpan.Zero);
                            c.JobRetryConfigurations =
                                jobRetryConfigurations
                                ?? new Dictionary<Type, RetryConfiguration>();
                        }
                    )
                ).RunServerAsync()
        );
    }
}
