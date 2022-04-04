using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Server;
using Baseline.Labourer.Tests.Scenarios.Internal.Wrappers;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Tests.Scenarios.Internal;

public abstract class BaseTest : IAsyncLifetime
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Guid _uniqueTestId = Guid.NewGuid();
    private readonly CancellationTokenSource _cancellationTokenSource =
        new CancellationTokenSource();

    protected QueueWrapper QueueWrapper { get; private set; }
    protected StoreWrapper StoreWrapper { get; private set; }
    protected TestDateTimeProvider TestDateTimeProvider { get; } = new();
    protected ILabourerClient Client { get; set; }

    protected BaseTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await QueueWrapper.DisposeAsync();
        await StoreWrapper.DisposeAsync();
        _cancellationTokenSource.Cancel();
    }

    protected async Task BootstrapAsync(QueueProvider queueProvider, StoreProvider storeProvider)
    {
        QueueWrapper = Queue(queueProvider);
        StoreWrapper = Store(storeProvider);

        await QueueWrapper.BootstrapAsync();
        await StoreWrapper.BootstrapAsync();

        var loggerFactory = LoggerFactory.Create(
            builder =>
            {
                builder.AddXUnit(_testOutputHelper);
            }
        );

        Client = new LabourerClient(
            new BaselineLabourerClientConfiguration
            {
                DateTimeProvider = TestDateTimeProvider,
                Queue = QueueWrapper.Queue,
                Store = StoreWrapper.Store,
                LoggerFactory = () => loggerFactory
            }
        );
        var server = new LabourerServer(
            new BaselineLabourerServerConfiguration
            {
                DateTimeProvider = TestDateTimeProvider,
                Queue = QueueWrapper.Queue,
                Store = StoreWrapper.Store,
                ShutdownTokenSource = _cancellationTokenSource,
                LoggerFactory = () => loggerFactory,
                ScheduledJobProcessorInterval = TimeSpan.FromMilliseconds(500)
            }
        );

#pragma warning disable CS4014
        Task.Run(async () => await server.RunServerAsync());
#pragma warning restore CS4014
    }

    private QueueWrapper Queue(QueueProvider queue)
    {
        return queue switch
        {
            QueueProvider.Memory => new MemoryQueueWrapper(_uniqueTestId),
            QueueProvider.SQLite => new SqliteQueueWrapper(_uniqueTestId),
            _ => throw new ArgumentOutOfRangeException(nameof(queue), queue, null)
        };
    }

    private StoreWrapper Store(StoreProvider store)
    {
        return store switch
        {
            StoreProvider.Memory => new MemoryStoreWrapper(_uniqueTestId),
            StoreProvider.SQLite => new SqliteStoreWrapper(_uniqueTestId),
            _ => throw new ArgumentOutOfRangeException(nameof(store), store, null)
        };
    }
}
