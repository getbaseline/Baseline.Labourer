namespace Baseline.Labourer.Tests
{
    public class ClientTest
    {
        protected LabourerClient Client { get; }

        protected TestMemoryBackingStore TestBackingStore { get; } = new TestMemoryBackingStore();

        protected TestDateTimeProvider TestDateTimeProvider { get; } = new TestDateTimeProvider();

        protected TestMemoryQueue TestMemoryQueue { get; }

        public ClientTest()
        {
            TestMemoryQueue = new TestMemoryQueue(TestDateTimeProvider);
            Client = new LabourerClient(
                new BaselineLabourerConfiguration
                {
                    Queue = TestMemoryQueue,
                    Store = new TestMemoryStore(TestBackingStore, TestDateTimeProvider)
                }
            );
        }
    }
}