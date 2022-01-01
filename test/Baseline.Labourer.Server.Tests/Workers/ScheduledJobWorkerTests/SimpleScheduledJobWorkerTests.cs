using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Tests;
using FluentAssertions;
using NCrontab;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests.Workers.ScheduledJobWorkerTests
{
    public class SimpleScheduledJobWorkerTests : ServerTest
    {
        public class TestScheduledJob : IJob
        {
            public static bool Ran = false;

            public ValueTask HandleAsync(CancellationToken cancellationToken)
            {
                Ran = true;
                return new ValueTask();
            }
        }

        public SimpleScheduledJobWorkerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            var context = GenerateServerContextAsync();

            Task.Run(
                async () =>
                    await new ScheduledJobDispatcherWorker.ScheduledJobDispatcherWorker(context, TestDateTimeProvider)
                        .RunAsync()
            );

            Task.Run(
                async () => await new JobProcessorWorker.JobProcessorWorker(context).RunAsync()
            );
        }

        [Fact]
        public async Task It_Processes_A_Scheduled_Task_That_Is_Past_Due_To_Run_And_Updates_The_Next_Run_Date()
        {
            // Arrange.
            var scheduledJobId = await Client.CreateOrUpdateScheduledJobAsync<TestScheduledJob>("test", "0 * * * *");

            TestStore.ScheduledJobs[scheduledJobId].NextRunDate = DateTime.UtcNow.AddHours(-1); // Force the job to run.

            // Assert.
            await AssertionUtils.RetryAsync(() =>
            {
                TestScheduledJob.Ran.Should().BeTrue();

                var currentDate = DateTime.UtcNow;
                var nextRunShouldBe = currentDate
                    .AddHours(1)
                    .AddMinutes(-currentDate.Minute)
                    .AddSeconds(-currentDate.Second);

                var scheduledJob = TestStore.ScheduledJobs[scheduledJobId];
                scheduledJob.LastRunDate.Should().BeCloseTo(currentDate, TimeSpan.FromSeconds(1));
                scheduledJob.NextRunDate.Should().BeCloseTo(nextRunShouldBe, TimeSpan.FromMinutes(1));
            });
        }

        [Fact]
        public async Task It_Processes_A_Scheduled_Job_That_Is_Due_To_Run_In_The__Future_When_Its_Next_Run_Date_Is_Passed()
        {
            // Arrange.
            var scheduledJobId = await Client.CreateOrUpdateScheduledJobAsync<TestScheduledJob>("test", "0 0 * * *");

            // Act.
            Task.Run(async () =>
            {
                await Task.Delay(500);
                TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddDays(1).Date.AddSeconds(3));
            });

            // Assert.
            await AssertionUtils.RetryAsync(() =>
            {
                TestScheduledJob.Ran.Should().BeTrue();
                
                var nextRunShouldBe = CrontabSchedule
                    .Parse("0 0 0 * * *", new CrontabSchedule.ParseOptions { IncludingSeconds = true })
                    .GetNextOccurrence(DateTime.UtcNow.AddDays(1).Date.AddSeconds(3));
                
                TestStore.ScheduledJobs[scheduledJobId].NextRunDate.Should().Be(nextRunShouldBe);
            });
        }

        [Fact]
        public async Task It_Can_Process_A_Scheduled_Job_Multiple_Times()
        {
            // Arrange.
            var scheduledJobId = await Client.CreateOrUpdateScheduledJobAsync<TestScheduledJob>("test", "0 0 * * *");

            for (int i = 1; i < 4; i++)
            {
                // Act.
                TestDateTimeProvider.SetUtcNow(DateTime.UtcNow.AddDays(i).Date.AddSeconds(3));
            
                // Assert.
                await AssertionUtils.RetryAsync(() =>
                {
                    TestScheduledJob.Ran.Should().BeTrue();
                
                    var nextRunShouldBe = CrontabSchedule
                        .Parse("0 0 0 * * *", new CrontabSchedule.ParseOptions { IncludingSeconds = true })
                        .GetNextOccurrence(DateTime.UtcNow.AddDays(i).Date.AddSeconds(3));
                
                    TestStore.ScheduledJobs[scheduledJobId].NextRunDate.Should().Be(nextRunShouldBe);
                });
            }
        }
    }
}