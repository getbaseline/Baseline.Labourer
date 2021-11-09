using System.Net;
using Baseline.Labourer.Internal.Utils;
using Baseline.Labourer.Tests;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Baseline.Labourer.Server.Tests;

public class ServerTest : IDisposable
{
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    protected readonly TestDispatchedJobStore TestJobStore = new TestDispatchedJobStore();

    protected readonly TestQueue TestQueue = new TestQueue();

    protected readonly TestServerStore TestServerStore = new TestServerStore();

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
            TestJobStore,
            TestQueue
        );
    }

    public async Task<ServerContext> GenerateServerContextAsync(int workers = 1)
    {
        var serverInstance = await TestServerStore.CreateServerAsync(new ServerInstance
        {
            Hostname = Dns.GetHostName(),
            Key = StringGenerationUtils.GenerateUniqueRandomString()
        }, CancellationToken.None);

        ServerId = serverInstance.Id;

        return new ServerContext
        {
            Activator = new DefaultJobActivator(),
            DispatchedJobStore = TestJobStore,
            Queue = TestQueue,
            ServerInstance = serverInstance,
            ServerStore = TestServerStore,
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
