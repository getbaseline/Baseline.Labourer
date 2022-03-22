namespace Baseline.Labourer.Tests;

public class ClientTest
{
    protected LabourerClient Client { get; }

    protected TestMemoryStoreDataContainer TestStoreDataContainer { get; } =
        new TestMemoryStoreDataContainer();

    protected TestDateTimeProvider TestDateTimeProvider { get; } = new TestDateTimeProvider();

    protected TestMemoryQueue TestMemoryQueue { get; }

    public ClientTest()
    {
        TestMemoryQueue = new TestMemoryQueue(TestDateTimeProvider);
        Client = new LabourerClient(
            new BaselineLabourerClientConfiguration
            {
                Queue = TestMemoryQueue,
                Store = new TestMemoryStore(TestStoreDataContainer, TestDateTimeProvider)
            }
        );
    }
}
