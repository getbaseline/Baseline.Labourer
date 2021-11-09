namespace Baseline.Labourer.Store.Memory;

/// <summary>
/// <see cref="MemoryServerStore" /> is an <see cref="IServerStore" /> implementation that persists server related entities in memory.
/// </summary>
public class MemoryServerStore : IServerStore
{
    private readonly SemaphoreSlim _serverSemaphore = new SemaphoreSlim(1);
    protected readonly List<ServerInstance> Servers = new List<ServerInstance>();
    protected readonly Dictionary<string, List<Worker>> ServerWorkers = new Dictionary<string, List<Worker>>();
    protected readonly Dictionary<string, List<DateTime>> ServerHeartbeats = new Dictionary<string, List<DateTime>>();

    /// <inheritdoc />
    public async Task<ServerInstance> CreateServerAsync(ServerInstance serverInstance, CancellationToken cancellationToken)
    {
        try
        {
            await _serverSemaphore.WaitAsync(cancellationToken);
            Servers.Add(serverInstance);
        }
        finally
        {
            _serverSemaphore.Release();
        }

        return serverInstance;
    }

    /// <inheritdoc />
    public async Task CreateServerHeartbeatAsync(string serverId, CancellationToken cancellationToken)
    {
        try
        {
            await _serverSemaphore.WaitAsync(cancellationToken);

            if (!ServerHeartbeats.ContainsKey(serverId))
            {
                ServerHeartbeats[serverId] = new List<DateTime>();
            }

            ServerHeartbeats[serverId].Add(DateTime.UtcNow);
        }
        finally
        {
            _serverSemaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<Worker> CreateWorkerAsync(Worker worker, CancellationToken cancellationToken)
    {
        try
        {
            await _serverSemaphore.WaitAsync(cancellationToken);

            if (!ServerWorkers.ContainsKey(worker.ServerInstanceId))
            {
                ServerWorkers[worker.ServerInstanceId] = new List<Worker>();
            }

            ServerWorkers[worker.ServerInstanceId].Add(worker);
        }
        finally
        {
            _serverSemaphore.Release();
        }

        return worker;
    }
}
