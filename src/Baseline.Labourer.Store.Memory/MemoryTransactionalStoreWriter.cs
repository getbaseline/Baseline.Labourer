using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;

namespace Baseline.Labourer.Store.Memory
{
    /// <summary>
    /// Memory based transactional store writer. Ensures all writes within a given context are atomic and all succeed or all fail.
    /// </summary>
    public class MemoryTransactionalStoreWriter : ITransactionalStoreWriter
    {
        private readonly MemoryBackingStore _memoryBackingStore;

        private readonly List<ServerInstance> _serverInstancesToCreate = new List<ServerInstance>();

        private readonly Dictionary<string, List<DateTime>> _heartbeatsToCreate = new Dictionary<string, List<DateTime>>();

        private readonly List<Worker> _workersToCreate = new List<Worker>();

        private readonly List<ScheduledJobDefinition> _scheduledJobsToCreate = new List<ScheduledJobDefinition>();

        private readonly Dictionary<string, List<Action<ScheduledJobDefinition>>> _scheduledJobUpdates =
            new Dictionary<string, List<Action<ScheduledJobDefinition>>>();

        private readonly List<string> _scheduledJobDeletions = new List<string>();

        private readonly List<DispatchedJobDefinition> _jobDefinitionsToCreate = new List<DispatchedJobDefinition>();
        
        private readonly Dictionary<string, List<Action<DispatchedJobDefinition>>> _jobDefinitionUpdates = 
            new Dictionary<string, List<Action<DispatchedJobDefinition>>>();

        public MemoryTransactionalStoreWriter(MemoryBackingStore memoryBackingStore)
        {
            _memoryBackingStore = memoryBackingStore;
        }

        /// <inheritdoc />
        public async ValueTask CommitAsync(CancellationToken cancellationToken)
        {
            using var _ = await _memoryBackingStore.AcquireStoreLockAsync();

            _memoryBackingStore.Servers.AddRange(_serverInstancesToCreate);

            foreach (var heartbeat in _heartbeatsToCreate)
            {
                if (!_memoryBackingStore.ServerHeartbeats.ContainsKey(heartbeat.Key))
                {
                    _memoryBackingStore.ServerHeartbeats[heartbeat.Key] = new List<DateTime>();
                }

                _memoryBackingStore.ServerHeartbeats[heartbeat.Key].AddRange(heartbeat.Value);
            }

            foreach (var worker in _workersToCreate)
            {
                if (!_memoryBackingStore.ServerWorkers.ContainsKey(worker.ServerInstanceId))
                {
                    _memoryBackingStore.ServerWorkers[worker.ServerInstanceId] = new List<Worker>();
                }

                _memoryBackingStore.ServerWorkers[worker.ServerInstanceId].Add(worker);
            }

            foreach (var scheduledJobAdd in _scheduledJobsToCreate)
            {
                _memoryBackingStore.ScheduledJobs.Add(scheduledJobAdd.Id, scheduledJobAdd);    
            }

            foreach (var scheduledJobUpdate in _scheduledJobUpdates)
            {
                _memoryBackingStore.ScheduledJobs.TryGetValue(scheduledJobUpdate.Key, out var scheduledJobToUpdate);

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
                _memoryBackingStore.ScheduledJobs.Remove(scheduledJobDeletion);
            }

            _memoryBackingStore.DispatchedJobs.AddRange(_jobDefinitionsToCreate);

            foreach (var jobUpdate in _jobDefinitionUpdates)
            {
                var jobToUpdate = _memoryBackingStore.DispatchedJobs.FirstOrDefault(job => job.Id == jobUpdate.Key);
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
        public ValueTask CreateServerAsync(ServerInstance serverInstance, CancellationToken cancellationToken)
        {
            _serverInstancesToCreate.Add(serverInstance);
            return new ValueTask();
        }

        /// <inheritdoc />
        public ValueTask CreateServerHeartbeatAsync(string serverId, CancellationToken cancellationToken)
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
            if (!_memoryBackingStore.ScheduledJobs.ContainsKey(scheduledJobDefinition.Id))
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
        public ValueTask UpdateJobRetriesAsync(string jobId, uint retries, CancellationToken cancellationToken)
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

        public ValueTask UpdateScheduledJobLastRunDateAsync(string jobId, DateTime? lastRunDate, CancellationToken cancellationToken)
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
}