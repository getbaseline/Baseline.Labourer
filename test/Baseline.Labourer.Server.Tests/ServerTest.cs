using Baseline.Labourer.Queue.Memory;
using Baseline.Labourer.Store.Memory;
using Baseline.Labourer.Tests;

namespace Baseline.Labourer.Server.Tests
{
    public class ServerTest
    {
        protected TestDispatchedJobStore TestJobStore = new TestDispatchedJobStore();

        protected TestQueue TestQueue = new TestQueue();
        
        public LabourerClient Client { get; }

        public ServerTest()
        {
            Client = new LabourerClient(
                new BaselineLabourerConfiguration(),
                TestJobStore,
                TestQueue
            );
        }
    }
}