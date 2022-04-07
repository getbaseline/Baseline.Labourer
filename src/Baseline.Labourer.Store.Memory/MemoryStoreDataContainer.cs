using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Models;

namespace Baseline.Labourer.Store.Memory;

/// <summary>
/// A collection of all entities and components that a store can manage. Its sole purpose is to provide a centralised place
/// to manage store state.
/// </summary>
public class MemoryStoreDataContainer
{
    private readonly SemaphoreSlim _semaphore = new(1);

    /// <summary>
    /// Gets the jobs that have been dispatched.
    /// </summary>
    public List<DispatchedJobDefinition> DispatchedJobs { get; } =
        new();

    /// <summary>
    /// Gets or sets the locks that have been made against resources.
    /// </summary>
    public Dictionary<string, List<MemoryLock>> Locks { get; } =
        new();

    /// <summary>
    /// Gets the log entries that have been created.
    /// </summary>
    public List<MemoryLogEntry> LogEntries { get; } = new();

    /// <summary>
    /// Gets the scheduled jobs that have been created.
    /// </summary>
    public Dictionary<string, ScheduledJobDefinition> ScheduledJobs { get; } =
        new();

    /// <summary>
    /// Gets the servers that have been created.
    /// </summary>
    public List<ServerInstance> Servers { get; } = new();

    /// <summary>
    /// Gets the server workers that have been created.
    /// </summary>
    public Dictionary<string, List<Worker>> ServerWorkers { get; } =
        new();

    /// <summary>
    /// Gets the server heartbeats that have been created.
    /// </summary>
    public Dictionary<string, List<DateTime>> ServerHeartbeats { get; } =
        new();

    /// <summary>
    /// Acquires a "lock" on the data source, preventing anyone else that calls this method from updating whilst the first callee has the lock.
    /// This isn't "idiot proof" - someone could just bypass this if they weren't to bother calling it. Don't do that. Please.
    /// </summary>
    public async Task<IDisposable> AcquireStoreLockAsync()
    {
        await _semaphore.WaitAsync();
        return new ComposableDisposable(() => _semaphore.Release());
    }
}
