using Baseline.Labourer.Store.Memory;

namespace Baseline.Labourer.Tests
{
    public class ClientTest
    {
        protected LabourerClient Client { get; }

        protected TestMemoryStore TestStore { get; } = new TestMemoryStore();

        protected TestQueue TestQueue { get; } = new TestQueue(new TestDateTimeProvider());

        public ClientTest()
        {
            Client = new LabourerClient(
                new BaselineLabourerConfiguration(),
                new MemoryResourceLocker(TestStore),
                new MemoryStoreWriterTransactionManager(TestStore),
                TestQueue
            );
        }
    }
}