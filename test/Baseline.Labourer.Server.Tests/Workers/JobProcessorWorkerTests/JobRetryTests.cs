using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests.Workers.JobProcessorWorkerTests
{
    public class JobRetryTests : ServerTest
    {
        public JobRetryTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Task.Run(
                async () => await new JobProcessorWorker.JobProcessorWorker(GenerateServerContextAsync()).RunAsync()
            );
        }


        public class CatastrophicErrorJob : IJob
        {
            public async Task HandleAsync(CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }
        }

        [Fact]
        public async Task It_Retries_A_Job_A_Maximum_Of_Three_Times_Before_Marking_The_Job_As_A_Catastrophic_Failure()
        {
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
            public Task HandleAsync(CancellationToken cancellationToken)
            {
                _executions++;

                if (_executions != 3) // Pass on the third retry.
                {
                    throw new System.NotImplementedException();
                }

                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task It_Completes_A_Job_Even_If_It_Fails_A_Couple_Of_Times()
        {
            // Act.
            var jobId = await Client.DispatchJobAsync<FailedJobThatCompletes>();

            // Assert.
            await AssertionUtils.RetryAsync(() =>
            {
                TestStore.AssertJobHasRetryCount(jobId, 2);
                TestStore.AssertStatusForJobIs(jobId, JobStatus.Complete);
            });
        }
    }
}