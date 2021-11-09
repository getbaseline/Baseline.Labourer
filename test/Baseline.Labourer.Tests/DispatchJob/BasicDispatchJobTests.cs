using Baseline.Labourer.Internal.Utils;
using Xunit;

namespace Baseline.Labourer.Tests.DispatchJob;

public class BasicDispatchJobTests : ClientTest
{
    public class BasicJob : IJob
    {
        public Task HandleAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }

    [Fact]
    public async Task When_Dispatching_A_Job_It_Records_Its_Initial_State_And_Sends_It_To_A_Queue()
    {
        // Act.
        await Client.DispatchJobAsync<BasicJob>();

        // Assert.
        var jobDefinition = DispatchedJobStore.AssertJobWithTypesStored(typeof(BasicJob));

        Queue.AssertMessageDispatched(
            new QueuedJob
            {
                SerializedDefinition = await SerializationUtils.SerializeToStringAsync(
                    jobDefinition,
                    CancellationToken.None
                )
            }
        );
    }
}
