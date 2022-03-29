using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer;

/// <summary>
/// Memory based transactional store writer. Ensures all writes within a given context are atomic and all succeed or all fail.
/// </summary>
public class MemoryTransactionalStoreWriter : ITransactionalStoreWriter
{
    private readonly MemoryStoreDataContainer _memoryStoreDataContainer;

    private readonly List<ServerInstance> _serverInstancesToCreate = new();

    private readonly Dictionary<string, List<DateTime>> _heartbeatsToCreate = new();

    private readonly List<Worker> _workersToCreate = new();

    private readonly List<ScheduledJobDefinition> _scheduledJobsToCreate = new();

    private readonly Dictionary<string, List<Action<ScheduledJobDefinition>>> _scheduledJobUpdates =
        new();

    private readonly List<string> _scheduledJobDeletions = new();

    private readonly List<DispatchedJobDefinition> _jobDefinitionsToCreate = new();

    private readonly Dictionary<
        string,
        List<Action<DispatchedJobDefinition>>
    > _jobDefinitionUpdates = new();

    private readonly List<MemoryLogEntry> _jobLogEntriesToAdd = new();

    public MemoryTransactionalStoreWriter(MemoryStoreDataContainer memoryStoreDataContainer)
    {
        _memoryStoreDataContainer = memoryStoreDataContainer;
    }

    /// <inheritdoc />
    public async ValueTask CommitAsync(CancellationToken cancellationToken)
    {
        using var _ = await _memoryStoreDataContainer.AcquireStoreLockAsync();

        _memoryStoreDataContainer.Servers.AddRange(_serverInstancesToCreate);

        foreach (var heartbeat in _heartbeatsToCreate)
        {
            if (!_memoryStoreDataContainer.ServerHeartbeats.ContainsKey(heartbeat.Key))
            {
                _memoryStoreDataContainer.ServerHeartbeats[heartbeat.Key] = new List<DateTime>();
            }

            _memoryStoreDataContainer.ServerHeartbeats[heartbeat.Key].AddRange(heartbeat.Value);
        }

        foreach (var worker in _workersToCreate)
        {
            if (!_memoryStoreDataContainer.ServerWorkers.ContainsKey(worker.ServerInstanceId))
            {
                _memoryStoreDataContainer.ServerWorkers[worker.ServerInstanceId] =
                    new List<Worker>();
            }

            _memoryStoreDataContainer.ServerWorkers[worker.ServerInstanceId].Add(worker);
        }

        foreach (var scheduledJobAdd in _scheduledJobsToCreate)
        {
            _memoryStoreDataContainer.ScheduledJobs.Add(scheduledJobAdd.Id, scheduledJobAdd);
        }

        foreach (var scheduledJobUpdate in _scheduledJobUpdates)
        {
            _memoryStoreDataContainer.ScheduledJobs.TryGetValue(
                scheduledJobUpdate.Key,
                out var scheduledJobToUpdate
            );

            if (scheduledJobToUpdate == null)
            {
                continue;
            }

            foreach (var update in scheduledJobUpdate.Value)
            {
                update(scheduledJobToUpdate);
            }
        }

        foreach (var scheduledJobDeletion in _scheduledJobDeletions)
        {
            _memoryStoreDataContainer.ScheduledJobs.Remove(scheduledJobDeletion);
        }

        _memoryStoreDataContainer.DispatchedJobs.AddRange(_jobDefinitionsToCreate);

        foreach (var jobUpdate in _jobDefinitionUpdates)
        {
            var jobToUpdate = _memoryStoreDataContainer.DispatchedJobs.FirstOrDefault(
                job => job.Id == jobUpdate.Key
            );
            if (jobToUpdate == null)
            {
                continue;
            }

            foreach (var update in jobUpdate.Value)
            {
                update(jobToUpdate);
            }
        }

        _memoryStoreDataContainer.LogEntries.AddRange(_jobLogEntriesToAdd);
    }

    /// <inheritdoc />
    public ValueTask CreateServerAsync(
        ServerInstance serverInstance,
        CancellationToken cancellationToken
    )
    {
        _serverInstancesToCreate.Add(serverInstance);
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CreateServerHeartbeatAsync(
        string serverId,
        CancellationToken cancellationToken
    )
    {
        if (!_heartbeatsToCreate.ContainsKey(serverId))
        {
            _heartbeatsToCreate.Add(serverId, new List<DateTime>());
        }

        _heartbeatsToCreate[serverId].Add(DateTime.UtcNow);

        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CreateWorkerAsync(Worker worker, CancellationToken cancellationToken)
    {
        _workersToCreate.Add(worker);
        return new ValueTask();
    }

    /// <summary>
    /// Disposes the memory store. This does nothing here because there isn't anything that needs tidying up.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CreateDispatchedJobAsync(
        DispatchedJobDefinition definition,
        CancellationToken cancellationToken
    )
    {
        _jobDefinitionsToCreate.Add(definition);
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask CreateOrUpdateScheduledJobAsync(
        ScheduledJobDefinition scheduledJobDefinition,
        CancellationToken cancellationToken
    )
    {
        if (!_memoryStoreDataContainer.ScheduledJobs.ContainsKey(scheduledJobDefinition.Id))
        {
            _scheduledJobsToCreate.Add(scheduledJobDefinition);
        }
        else
        {
            UpdateScheduledJob(
                scheduledJobDefinition.Id,
                scheduledJob =>
                {
                    scheduledJob.Type = scheduledJobDefinition.Type;
                    scheduledJob.ParametersType = scheduledJobDefinition.ParametersType;
                    scheduledJob.SerializedParameters = scheduledJobDefinition.SerializedParameters;
                    scheduledJob.CronExpression = scheduledJobDefinition.CronExpression;
                    scheduledJob.UpdatedAt = scheduledJobDefinition.UpdatedAt;
                }
            );
        }

        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask DeleteScheduledJobAsync(string id, CancellationToken cancellationToken)
    {
        _scheduledJobDeletions.Add(id);
        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask LogEntryForJobAsync(
        string jobId,
        LogLevel logLevel,
        string message,
        Exception? exception,
        CancellationToken cancellationToken
    )
    {
        _jobLogEntriesToAdd.Add(
            new MemoryLogEntry
            {
                JobId = jobId,
                LogLevel = logLevel,
                Message = message,
                Exception = exception
            }
        );

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask UpdateJobRetriesAsync(
        string jobId,
        uint retries,
        CancellationToken cancellationToken
    )
    {
        UpdateJob(jobId, job => job.Retries = retries);
        return new ValueTask();
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

        return new ValueTask();
    }

    /// <inheritdoc />
    public ValueTask UpdateScheduledJobNextRunDateAsync(
        string jobId,
        DateTime nextRunDate,
        CancellationToken cancellationToken
    )
    {
        UpdateScheduledJob(jobId, job => job.NextRunDate = nextRunDate);
        return new ValueTask();
    }

    public ValueTask UpdateScheduledJobLastRunDateAsync(
        string jobId,
        DateTime? lastRunDate,
        CancellationToken cancellationToken
    )
    {
        UpdateScheduledJob(jobId, job => job.LastRunDate = lastRunDate);
        return new ValueTask();
    }

    private void UpdateJob(string jobId, Action<DispatchedJobDefinition> updateAction)
    {
        if (!_jobDefinitionUpdates.ContainsKey(jobId))
        {
            _jobDefinitionUpdates.Add(jobId, new List<Action<DispatchedJobDefinition>>());
        }

        _jobDefinitionUpdates[jobId].Add(updateAction);
    }

    private void UpdateScheduledJob(string jobId, Action<ScheduledJobDefinition> updateAction)
    {
        if (!_scheduledJobUpdates.ContainsKey(jobId))
        {
            _scheduledJobUpdates[jobId] = new List<Action<ScheduledJobDefinition>>();
        }

        _scheduledJobUpdates[jobId].Add(updateAction);
    }
}
