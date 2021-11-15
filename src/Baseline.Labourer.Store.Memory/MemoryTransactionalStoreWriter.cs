using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Contracts;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Store.Memory
{
    /// <summary>
    /// Memory based transactional store writer. Ensures all writes within a given context are atomic and all succeed or all fail.
    /// </summary>
    public class MemoryTransactionalStoreWriter : ITransactionalStoreWriter
    {
        private readonly MemoryStore _memoryStore;

        private readonly List<ServerInstance> _serverInstancesToCreate = new List<ServerInstance>();

        private readonly Dictionary<string, List<DateTime>> _heartbeatsToCreate = new Dictionary<string, List<DateTime>>();

        private readonly List<Worker> _workersToCreate = new List<Worker>();

        private readonly List<ScheduledJobDefinition> _scheduledJobsToCreate = new List<ScheduledJobDefinition>();

        private readonly Dictionary<string, List<Action<ScheduledJobDefinition>>> _scheduledJobUpdates =
            new Dictionary<string, List<Action<ScheduledJobDefinition>>>();

        private readonly List<DispatchedJobDefinition> _jobDefinitionsToCreate = new List<DispatchedJobDefinition>();
        private readonly Dictionary<string, List<Action<DispatchedJobDefinition>>> _jobDefinitionUpdates = 
            new Dictionary<string, List<Action<DispatchedJobDefinition>>>();

        public MemoryTransactionalStoreWriter(MemoryStore memoryStore)
        {
            _memoryStore = memoryStore;
        }

        /// <inheritdoc />
        public async ValueTask CommitAsync(CancellationToken cancellationToken)
        {
            using var _ = await _memoryStore.AcquireLockAsync();

            _memoryStore.Servers.AddRange(_serverInstancesToCreate);

            foreach (var heartbeat in _heartbeatsToCreate)
            {
                if (!_memoryStore.ServerHeartbeats.ContainsKey(heartbeat.Key))
                {
                    _memoryStore.ServerHeartbeats[heartbeat.Key] = new List<DateTime>();
                }

                _memoryStore.ServerHeartbeats[heartbeat.Key].AddRange(heartbeat.Value);
            }

            foreach (var worker in _workersToCreate)
            {
                if (!_memoryStore.ServerWorkers.ContainsKey(worker.ServerInstanceId))
                {
                    _memoryStore.ServerWorkers[worker.ServerInstanceId] = new List<Worker>();
                }

                _memoryStore.ServerWorkers[worker.ServerInstanceId].Add(worker);
            }

            _memoryStore.ScheduledJobs.AddRange(_scheduledJobsToCreate);

            foreach (var scheduledJobUpdate in _scheduledJobUpdates)
            {
                var scheduledJobToUpdate =
                    _memoryStore.ScheduledJobs.FirstOrDefault(job => job.Id == scheduledJobUpdate.Key);
                
                if (scheduledJobToUpdate == null)
                {
                    continue;
                }

                foreach (var update in scheduledJobUpdate.Value)
                {
                    update(scheduledJobToUpdate);
                }
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
            return new ValueTask<ServerInstance>(serverInstance);
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
        public ValueTask<Worker> CreateWorkerAsync(Worker worker, CancellationToken cancellationToken)
        {
            _workersToCreate.Add(worker);
            return new ValueTask<Worker>();
        }

        /// <summary>
        /// Disposes the memory store. This does nothing here because there isn't anything that needs tidying up.
        /// </summary>
        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        /// <inheritdoc />
        public ValueTask<DispatchedJobDefinition> CreateDispatchedJobDefinitionAsync(
            DispatchedJobDefinition definition,
            CancellationToken cancellationToken
        )
        {
            _jobDefinitionsToCreate.Add(definition);
            return new ValueTask<DispatchedJobDefinition>(definition);
        }

        /// <inheritdoc />
        public ValueTask<ScheduledJobDefinition> CreateScheduledJobDefinitionAsync(
            ScheduledJobDefinition scheduledJobDefinition,
            CancellationToken cancellationToken
        )
        {
            _scheduledJobsToCreate.Add(scheduledJobDefinition);
            return new ValueTask<ScheduledJobDefinition>(scheduledJobDefinition);
        }

        /// <inheritdoc />
        public ValueTask UpdateJobRetriesAsync(string jobId, int retries, CancellationToken cancellationToken)
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