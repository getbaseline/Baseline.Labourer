using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Baseline.Labourer.Tests
{
    public class ScheduleJobTests : ClientTest
    {
        [Fact]
        public async Task It_Successfully_Creates_The_Scheduled_Job_Record()
        {
            // Arrange.
            var cronExpression = "* * * * *";
            
            // Act.
            var scheduledJobId = await Client.CreateOrUpdateScheduledJobAsync<BasicJob>("created-job", cronExpression);

            // Assert.
            scheduledJobId.Should().Be("scheduled-job:created-job");
            TestStore.AssertScheduledJobExists(scheduledJobId, cronExpression);
        }

        public class TestScheduledJob : IJob
        {
            public ValueTask HandleAsync(CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }
        }

        public class TestScheduledParameters
        {
            public string Name { get; } = "foo";
        }
        
        public class TestScheduledJobWithParameters : IJob<TestScheduledParameters>
        {
            public Task HandleAsync(TestScheduledParameters parameters, CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }
        }

        [Fact]
        public async Task It_Can_Update_A_Scheduled_Job_Record()
        {
            // Arrange.
            var scheduledJobId = await Client.CreateOrUpdateScheduledJobAsync<TestScheduledJob>(
                "update-scheduled-job", 
                "* * * * *"
            );
            
            // Act.
            await Client.CreateOrUpdateScheduledJobAsync<TestScheduledParameters, TestScheduledJobWithParameters>(
                scheduledJobId,
                "0 * * * *",
                new TestScheduledParameters()
            );

            // Assert.
            TestStore.AssertScheduledJobExists(
                scheduledJobId,
                "0 * * * *",
                typeof(TestScheduledJobWithParameters).AssemblyQualifiedName,
                typeof(TestScheduledParameters).AssemblyQualifiedName,
                "{\"Name\":\"foo\"}"
            );
        }
    }
}