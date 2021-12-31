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
            var scheduledJobId = await Client.ScheduleJobAsync<BasicJob>("created-job", cronExpression);

            // Assert.
            scheduledJobId.Should().Be("scheduled-job:created-job");
            TestStore.AssertScheduledJobCreated(scheduledJobId, cronExpression);
        }
    }
}