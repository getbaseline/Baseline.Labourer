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
            var scheduledJobId = await Client.ScheduleJobAsync<BasicJob>(cronExpression);

            // Assert.
            scheduledJobId.Should().NotBeNullOrWhiteSpace();
            TestStore.AssertScheduledJobCreated(scheduledJobId, cronExpression);
        }
    }
}