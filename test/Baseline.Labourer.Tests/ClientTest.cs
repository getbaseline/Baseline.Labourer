using Baseline.Labourer.Store.Memory;

namespace Baseline.Labourer.Tests
{
    public class ClientTest
    {
        protected LabourerClient Client { get; }

        protected TestMemoryStore TestStore { get; } = new TestMemoryStore();

        protected TestMemoryQueue TestMemoryQueue { get; } = new TestMemoryQueue(new TestDateTimeProvider());

        public ClientTest()
        {
            Client = new LabourerClient(
                new BaselineLabourerConfiguration(),
                new MemoryResourceLocker(TestStore),
                new MemoryStoreWriterTransactionManager(TestStore),
                TestMemoryQueue
            );
        }
    }
}