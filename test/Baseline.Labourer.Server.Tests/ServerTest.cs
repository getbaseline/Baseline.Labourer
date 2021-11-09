using System.Net;
using Baseline.Labourer.Internal.Utils;
using Baseline.Labourer.Store.Memory;
using Baseline.Labourer.Tests;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests;

public class ServerTest : IDisposable
{
    private CancellationTokenSource _cancellationTokenSource = new();

    protected readonly TestQueue TestQueue = new();

    protected readonly TestMemoryStore TestStore = new();

    protected readonly ILoggerFactory TestLoggerFactory;

    protected string ServerId;

    public LabourerClient Client { get; }

    public ServerTest(ITestOutputHelper testOutputHelper)
    {
        TestLoggerFactory = LoggerFactory.Create(logger =>
        {
            logger
                .AddXUnit(testOutputHelper)
                .SetMinimumLevel(LogLevel.Debug);
        });

        Client = new LabourerClient(
            new BaselineLabourerConfiguration
            {
                LoggerFactory = () => TestLoggerFactory
            },
            new MemoryStoreWriterTransactionManager(TestStore),
            TestQueue
        );
    }

    public async Task<ServerContext> GenerateServerContextAsync(int workers = 1)
    {
        var serverInstance = new ServerInstance
        {
            Hostname = Dns.GetHostName(),
            Key = StringGenerationUtils.GenerateUniqueRandomString()
        };
        TestStore.Servers.Add(serverInstance);

        ServerId = serverInstance.Id;

        return new ServerContext
        {
            Activator = new DefaultJobActivator(),
            JobLogStore = new MemoryJobLogStore(TestStore),
            Queue = TestQueue,
            ServerInstance = serverInstance,
            StoreWriterTransactionManager = new MemoryStoreWriterTransactionManager(TestStore),
            ShutdownTokenSource = _cancellationTokenSource,
            LoggerFactory = TestLoggerFactory,
            WorkersToRun = workers
        };
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
    }
}
