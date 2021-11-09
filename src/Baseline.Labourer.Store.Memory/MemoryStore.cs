namespace Baseline.Labourer.Store.Memory;

/// <summary>
/// <see cref="MemoryStore"/> is a collection of all entities and components that a store can manage. Its sole purpose is to provide a centralised place
/// to manage store state.
/// </summary>
public class MemoryStore
{
    private readonly SemaphoreSlim _semaphore = new(1);

    public List<DispatchedJobDefinition> DispatchedJobs = new();

    public List<MemoryLogEntry> LogEntries = new();

    public List<ServerInstance> Servers = new();

    public Dictionary<string, List<Worker>> ServerWorkers = new();

    public Dictionary<string, List<DateTime>> ServerHeartbeats = new();

    public async Task<IDisposable> AcquireLockAsync()
    {
        await _semaphore.WaitAsync();
        return new ComposableDisposable(() => _semaphore.Release());
    }
}
