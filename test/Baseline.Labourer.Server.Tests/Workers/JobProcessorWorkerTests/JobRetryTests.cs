using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Tests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests.Workers.JobProcessorWorkerTests
{
    public class JobRetryTests : ServerTest
    {
        public JobRetryTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }


        public class CatastrophicErrorJob : IJob
        {
            public async ValueTask HandleAsync(CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
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
            await AssertionUtils.RetryAsync(() =>
            {
                TestStore.AssertJobHasRetryCount(jobId, 3);
                TestStore.AssertStatusForJobIs(jobId, JobStatus.FailedExceededMaximumRetries);
            });
        }

        public class FailedJobThatCompletes : IJob
        {
            private static int _executions = 0;

            public ValueTask HandleAsync(CancellationToken cancellationToken)
            {
                _executions++;

                if (_executions != 3) // Pass on the third retry.
                {
                    throw new System.NotImplementedException();
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
            await AssertionUtils.RetryAsync(() =>
            {
                TestStore.AssertJobHasRetryCount(jobId, 2);
                TestStore.AssertStatusForJobIs(jobId, JobStatus.Complete);
            });
        }

        [Fact]
        public async Task The_Default_Retry_Amount_Can_Be_Changed()
        {
            // Arrange.
            RunWorker(new RetryConfiguration(10, TimeSpan.Zero));

            // Act.
            var jobId = await Client.DispatchJobAsync<CatastrophicErrorJob>();

            // Assert.
            await AssertionUtils.RetryAsync(() =>
            {
                TestStore.AssertJobHasRetryCount(jobId, 10);
                TestStore.AssertStatusForJobIs(jobId, JobStatus.FailedExceededMaximumRetries);
            });
        }

        public class JobWithChangedRetryAmountThatCatastrophicallyErrors : IJob
        {
            public ValueTask HandleAsync(CancellationToken cancellationToken)
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
                    { typeof(JobWithChangedRetryAmountThatCatastrophicallyErrors), new RetryConfiguration(2, TimeSpan.Zero) }
                }
            );

            // Act.
            var standardJobId = await Client.DispatchJobAsync<CatastrophicErrorJob>();
            var changedJobId = await Client.DispatchJobAsync<JobWithChangedRetryAmountThatCatastrophicallyErrors>();

            // Assert.
            await AssertionUtils.RetryAsync(() =>
            {
                TestStore.AssertJobHasRetryCount(standardJobId, 5);
                TestStore.AssertStatusForJobIs(standardJobId, JobStatus.FailedExceededMaximumRetries);
                TestStore.AssertJobHasRetryCount(changedJobId, 2);
                TestStore.AssertStatusForJobIs(changedJobId, JobStatus.FailedExceededMaximumRetries);
            });
        }

        [Fact]
        public async Task The_Default_Retry_Timeout_Can_Be_Changed_For_All_Jobs()
        {
            // Arrange.
            RunWorker(new RetryConfiguration(5, TimeSpan.FromSeconds(10)));

            // Act.
            var jobId = await Client.DispatchJobAsync<CatastrophicErrorJob>();

            // Assert.
            await AssertionUtils.RetryAsync(() =>
            {
                TestQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(jobId, 0, TimeSpan.Zero);
            });
            
            TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddSeconds(11));

            await AssertionUtils.RetryAsync(() =>
            {
                TestStore.AssertStatusForJobIs(jobId, JobStatus.Failed);
                TestQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(jobId, 1, TimeSpan.FromSeconds(10));
            });
            
            TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddSeconds(22));

            await AssertionUtils.RetryAsync(() =>
            {
                TestStore.AssertStatusForJobIs(jobId, JobStatus.Failed);
                TestQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(jobId, 2, TimeSpan.FromSeconds(10));
            });
            
            TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddSeconds(33));

            await AssertionUtils.RetryAsync(() =>
            {
                TestStore.AssertStatusForJobIs(jobId, JobStatus.Failed);
                TestQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(jobId, 3, TimeSpan.FromSeconds(10));
            });
            
            TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddSeconds(44));

            await AssertionUtils.RetryAsync(() =>
            {
                TestStore.AssertStatusForJobIs(jobId, JobStatus.Failed);
                TestQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(jobId, 4, TimeSpan.FromSeconds(10));
            });
            
            TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddSeconds(55));

            await AssertionUtils.RetryAsync(() =>
            {
                TestStore.AssertStatusForJobIs(jobId, JobStatus.FailedExceededMaximumRetries);
                TestQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(jobId, 5, TimeSpan.FromSeconds(10));
            });
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
                        new RetryConfiguration(1, TimeSpan.FromSeconds(60) )
                    }
                }
            );

            // Act.
            var standardJobId = await Client.DispatchJobAsync<CatastrophicErrorJob>();
            var changedJobId = await Client.DispatchJobAsync<JobWithChangedRetryAmountThatCatastrophicallyErrors>();

            // Assert.
            await AssertionUtils.RetryAsync(() =>
            {
                TestStore.AssertJobHasRetryCount(changedJobId, 1);
                TestStore.AssertStatusForJobIs(changedJobId, JobStatus.Failed);
                
                // Verify the standard job with the no delay has been completed (and failed).
                TestStore.AssertStatusForJobIs(standardJobId, JobStatus.FailedExceededMaximumRetries);
                TestQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(standardJobId, 5, TimeSpan.Zero);
            });
            
            TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddSeconds(70)); // Mimic the timeout actually occurring.

            await AssertionUtils.RetryAsync(() =>
            {
                // Verify the long delayed job has been completed (and failed).
                TestStore.AssertStatusForJobIs(changedJobId, JobStatus.FailedExceededMaximumRetries);
                TestQueue.AssertJobMessageRemovedOnCompletionWithIdRetryCountAndDelay(changedJobId, 1, TimeSpan.FromSeconds(60));
            });
        }

        private void RunWorker(
            RetryConfiguration defaultRetryConfiguration = null,
            Dictionary<Type, RetryConfiguration> jobRetryConfigurations = null
        )
        {
            Task.Run(
                async () => await new JobProcessorWorker.JobProcessorWorker(
                    GenerateServerContextAsync(c =>
                    {
                        c.DefaultRetryConfiguration = defaultRetryConfiguration ?? new RetryConfiguration(3, TimeSpan.Zero);
                        c.JobRetryConfigurations = jobRetryConfigurations ?? new Dictionary<Type, RetryConfiguration>();
                    })
                ).RunAsync()
            );
        }
    }
}
