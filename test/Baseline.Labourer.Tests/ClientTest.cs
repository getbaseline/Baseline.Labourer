namespace Baseline.Labourer.Tests
{
    public class ClientTest
    {
        protected LabourerClient Client { get; }

        protected TestDispatchedJobStore DispatchedJobStore { get; } = new TestDispatchedJobStore();

        protected TestQueue Queue { get; } = new TestQueue();

        public ClientTest()
        {
            Client = new LabourerClient(
                new BaselineLabourerConfiguration(), 
                DispatchedJobStore, 
                Queue
            );
        }
    }
}