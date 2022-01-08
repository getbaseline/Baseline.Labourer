using Baseline.Labourer.Store.Memory;

namespace Baseline.Labourer.Tests
{
    public class ClientTest
    {
        protected LabourerClient Client { get; }

        protected TestMemoryStore TestStore { get; } = new TestMemoryStore();

        protected TestDateTimeProvider TestDateTimeProvider { get; } = new TestDateTimeProvider();

        protected TestMemoryQueue TestMemoryQueue { get; }

        public ClientTest()
        {
            TestMemoryQueue = new TestMemoryQueue(TestDateTimeProvider);
            Client = new LabourerClient(
                new BaselineLabourerConfiguration(),
                new TestMemoryResourceLocker(TestStore, TestDateTimeProvider),
                new MemoryStoreWriterTransactionManager(TestStore),
                TestMemoryQueue
            );
        }
    }
}