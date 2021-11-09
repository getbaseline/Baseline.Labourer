using Baseline.Labourer.Contracts;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Store.Memory;

public class MemoryTransactionalStoreWriter : ITransactionalStoreWriter
{
    private readonly MemoryStore _memoryStore;

    private readonly List<ServerInstance> _serverInstancesToCreate = new();

    private readonly Dictionary<string, List<DateTime>> _heartbeatsToCreate = new();

    private readonly List<Worker> _workersToCreate = new();

    private readonly List<DispatchedJobDefinition> _jobDefinitionsToCreate = new();
    private readonly Dictionary<string, List<Action<DispatchedJobDefinition>>> _jobDefinitionUpdates = new();

    public MemoryTransactionalStoreWriter(MemoryStore memoryStore)
    {
        _memoryStore = memoryStore;
    }

    public async ValueTask CommitAsync(CancellationToken cancellationToken)
    {
        using var _ = await _memoryStore.AcquireLockAsync();

        _memoryStore.Servers.AddRange(_serverInstancesToCreate);

        foreach (var heartbeat in _heartbeatsToCreate)
        {
            if (!_memoryStore.ServerHeartbeats.ContainsKey(heartbeat.Key))
            {
                _memoryStore.ServerHeartbeats[heartbeat.Key] = new();
            }

            _memoryStore.ServerHeartbeats[heartbeat.Key].AddRange(heartbeat.Value);
        }

        foreach (var worker in _workersToCreate)
        {
            if (!_memoryStore.ServerWorkers.ContainsKey(worker.ServerInstanceId))
            {
                _memoryStore.ServerWorkers[worker.ServerInstanceId] = new();
            }

            _memoryStore.ServerWorkers[worker.ServerInstanceId].Add(worker);
        }

        _memoryStore.DispatchedJobs.AddRange(_jobDefinitionsToCreate);

        foreach (var jobUpdate in _jobDefinitionUpdates)
        {
            var jobToUpdate = _memoryStore.DispatchedJobs.FirstOrDefault(job => job.Id == jobUpdate.Key);
            if (jobToUpdate == null)
            {
                continue;
            }

            foreach (var update in jobUpdate.Value)
            {
                update(jobToUpdate);
            }
        }
    }

    /// <inheritdoc />
    public ValueTask<ServerInstance> CreateServerAsync(ServerInstance serverInstance, CancellationToken cancellationToken)
    {
        _serverInstancesToCreate.Add(serverInstance);
        return ValueTask.FromResult(serverInstance);
    }

    /// <inheritdoc />
    public ValueTask CreateServerHeartbeatAsync(string serverId, CancellationToken cancellationToken)
    {
        if (!_heartbeatsToCreate.ContainsKey(serverId))
        {
            _heartbeatsToCreate.Add(serverId, new List<DateTime>());
        }

        _heartbeatsToCreate[serverId].Add(DateTime.UtcNow);

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<Worker> CreateWorkerAsync(Worker worker, CancellationToken cancellationToken)
    {
        _workersToCreate.Add(worker);
        return ValueTask.FromResult(worker);
    }

    /// <summary>
    /// Disposes the memory store. This does nothing here because there isn't anything that needs tidying up.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<DispatchedJobDefinition> SaveDispatchedJobDefinitionAsync(
        DispatchedJobDefinition definition,
        CancellationToken cancellationToken
    )
    {
        _jobDefinitionsToCreate.Add(definition);
        return ValueTask.FromResult(definition);
    }

    /// <inheritdoc />
    public ValueTask UpdateJobStateAsync(
        string jobId,
        JobStatus jobStatus,
        DateTime? finishedDate,
        CancellationToken cancellationToken = default
    )
    {
        UpdateJob(
            jobId,
            job =>
            {
                job.Status = jobStatus;
                job.FinishedAt = finishedDate;
            }
        );

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask UpdateJobRetriesAsync(string jobId, int retries, CancellationToken cancellationToken)
    {
        UpdateJob(jobId, job => job.Retries = retries);
        return ValueTask.CompletedTask;
    }

    private void UpdateJob(string jobId, Action<DispatchedJobDefinition> updateAction)
    {
        if (!_jobDefinitionUpdates.ContainsKey(jobId))
        {
            _jobDefinitionUpdates.Add(jobId, new List<Action<DispatchedJobDefinition>>());
        }

        _jobDefinitionUpdates[jobId].Add(updateAction);
    }
}
