using Baseline.Labourer.Queue.Memory;
using Baseline.Labourer.Store.Memory;

namespace Baseline.Labourer.Server.Tests
{
    public class ServerTest
    {
        protected MemoryJobStore MemoryJobStore = new MemoryJobStore();

        protected MemoryQueue MemoryQueue = new MemoryQueue();
        
        public LabourerClient Client { get; }

        public ServerTest()
        {
            Client = new LabourerClient(
                new BaselineLabourerConfiguration(),
                MemoryJobStore,
                MemoryQueue
            );
        }
    }
}