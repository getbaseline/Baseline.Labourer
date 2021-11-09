using Baseline.Labourer.Store.Memory;

namespace Baseline.Labourer.Tests;

public class ClientTest
{
    protected LabourerClient Client { get; }

    protected TestMemoryStore TestStore { get; } = new TestMemoryStore();

    protected TestQueue TestQueue { get; } = new TestQueue();

    public ClientTest()
    {
        Client = new LabourerClient(
            new BaselineLabourerConfiguration(),
            new MemoryStoreWriterTransactionManager(TestStore),
            TestQueue
        );
    }
}
