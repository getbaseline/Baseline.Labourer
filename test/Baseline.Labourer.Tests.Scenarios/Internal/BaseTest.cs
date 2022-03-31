using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Server;
using Baseline.Labourer.Tests.Scenarios.Setup;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Baseline.Labourer.Tests.Scenarios.Internal;

public abstract class BaseTest : IAsyncLifetime
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Guid _uniqueTestId = Guid.NewGuid();
    private List<Func<Task>> _onDispose = new List<Func<Task>>();

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
        foreach (var disposalTask in _onDispose)
        {
            await disposalTask();
        }
    }

    protected async Task BootstrapAsync(QueueProvider queueProvider, StoreProvider storeProvider)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var queue = Queue(queueProvider);
        var store = Store(storeProvider);

        await queue.BootstrapAsync();
        await store.BootstrapAsync();

        var loggerFactory = LoggerFactory.Create(
            builder =>
            {
                builder.AddXUnit(_testOutputHelper);
            }
        );

        Client = new LabourerClient(
            new BaselineLabourerClientConfiguration
            {
                Queue = queue,
                Store = store,
                LoggerFactory = () => loggerFactory
            }
        );
        var server = new LabourerServer(
            new BaselineLabourerServerConfiguration
            {
                Queue = queue,
                Store = store,
                ShutdownTokenSource = cancellationTokenSource,
                LoggerFactory = () => loggerFactory
            }
        );

        Task.Run(async () => await server.RunServerAsync());

        _onDispose.Add(
            () =>
            {
                cancellationTokenSource.Cancel();
                return Task.CompletedTask;
            }
        );
    }

    protected IQueue Queue(QueueProvider queue)
    {
        return queue switch
        {
            QueueProvider.Memory => new MemoryQueue(),
            QueueProvider.SQLite => new SqliteQueue($"Data Source={_uniqueTestId};")
        };
    }

    protected IStore Store(StoreProvider store)
    {
        return store switch
        {
            StoreProvider.Memory => new MemoryStore(),
            StoreProvider.SQLite => new SqliteStore($"Data Source={_uniqueTestId};")
        };
    }
}
