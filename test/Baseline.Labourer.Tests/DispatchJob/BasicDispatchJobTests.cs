using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Utils;
using Xunit;

namespace Baseline.Labourer.Tests.DispatchJob
{
    public class BasicDispatchJobTests : ClientTest
    {
        public class BasicJobParameters
        {
            
        }
        
        public class BasicJob : IJob<BasicJobParameters>
        {
            public Task HandleAsync(BasicJobParameters parameters, CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }
        }

        [Fact]
        public async Task When_Dispatching_A_Job_It_Records_Its_Initial_State_And_Sends_It_To_A_Queue()
        {
            // Act.
            await Client.DispatchJobAsync<BasicJobParameters, BasicJob>(
                new BasicJobParameters(),
                CancellationToken.None
            );
            
            // Assert.
            var jobDefinition = DispatchedJobStore.AssertJobWithTypesStored(
                typeof(BasicJob), 
                typeof(BasicJobParameters)
            );

            Queue.AssertMessageDispatched(
                new QueuedJob
                {
                    Type = QueuedMessageType.UserEnqueuedJob,
                    SerializedDefinition = await SerializationUtils.SerializeToStringAsync(
                        jobDefinition, 
                        CancellationToken.None
                    )
                }
            );
        }
    }
}