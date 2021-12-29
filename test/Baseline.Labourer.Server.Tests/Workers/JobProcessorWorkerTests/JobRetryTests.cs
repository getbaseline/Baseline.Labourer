using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
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
            RunWorker(new RetryConfiguration(5, TimeSpan.FromSeconds(1)));

            var startDate = DateTime.UtcNow;
            
            // Act.
            var jobId = await Client.DispatchJobAsync<CatastrophicErrorJob>();
            
            // Assert.
            await AssertionUtils.RetryAsync(() =>
            {
                // refactor to utilise the date time provider to prevent really welly long tests
                TestQueue.AssertJobDispatchedWithIdRetryCountAndDelay(jobId, 1, TimeSpan.FromSeconds(1));
                TestQueue.AssertJobDispatchedWithIdRetryCountAndDelay(jobId, 2, TimeSpan.FromSeconds(1));
                TestQueue.AssertJobDispatchedWithIdRetryCountAndDelay(jobId, 3, TimeSpan.FromSeconds(1));
                TestQueue.AssertJobDispatchedWithIdRetryCountAndDelay(jobId, 4, TimeSpan.FromSeconds(1));
                TestQueue.AssertJobDispatchedWithIdRetryCountAndDelay(jobId, 5, TimeSpan.FromSeconds(1));
                TestStore.AssertJobHasRetryCount(jobId, 5);
                DateTime.UtcNow.Should().BeCloseTo(startDate.AddSeconds(10), TimeSpan.FromMilliseconds(999));
            }, 25, 500);
        }

        [Fact]
        public async Task The_Retry_Timeout_Can_Be_Changed_Per_Job()
        {
            // Arrange.
            RunWorker(
                new RetryConfiguration(5, TimeSpan.FromSeconds(1)),
                new Dictionary<Type, RetryConfiguration>
                {
                    { typeof(JobWithChangedRetryAmountThatCatastrophicallyErrors), new RetryConfiguration(1, TimeSpan.FromSeconds(10) )}
                }
            );
            
            // Act.
            var standardJobId = await Client.DispatchJobAsync<CatastrophicErrorJob>();
            var changedJobId = await Client.DispatchJobAsync<JobWithChangedRetryAmountThatCatastrophicallyErrors>();
            
            // Assert.
            await AssertionUtils.RetryAsync(() =>
            {
                TestQueue.AssertJobDispatchedWithIdRetryCountAndDelay(changedJobId, 1, TimeSpan.FromSeconds(10));
                TestStore.AssertJobHasRetryCount(changedJobId, 1);
            }, 50, 1000);
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