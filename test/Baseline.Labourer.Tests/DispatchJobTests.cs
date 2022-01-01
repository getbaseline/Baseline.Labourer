using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Utils;
using Xunit;

namespace Baseline.Labourer.Tests
{
    public class DispatchJobTests : ClientTest
    {
        [Fact]
        public async Task When_Dispatching_A_Job_It_Records_Its_Initial_State_And_Sends_It_To_A_Queue()
        {
            // Act.
            await Client.DispatchJobAsync<BasicJob>();

            // Assert.
            var jobDefinition = TestStore.AssertJobWithTypesStored(typeof(BasicJob));

            var serialisedJobDefinition = await SerializationUtils.SerializeToStringAsync(
                jobDefinition,
                CancellationToken.None
            );
            
            TestMemoryQueue.AssertMessageDispatched(j => j.SerializedDefinition == serialisedJobDefinition);
        }
    }
}