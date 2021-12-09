using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Contracts;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Contracts;
using Baseline.Labourer.Internal.Utils;

namespace Baseline.Labourer
{
    /// <summary>
    /// LabourerClient is the default (and ideally only) implementation of the <see cref="ILabourerClient"/> interface.
    /// </summary>
    public class LabourerClient : ILabourerClient
    {
        private readonly BaselineLabourerConfiguration _configuration;
        private readonly IResourceLocker _resourceLocker;
        private readonly IStoreWriterTransactionManager _storeWriterTransactionManager;
        private readonly JobDispatcher _jobDispatcher;
        private readonly IDateTimeProvider _dateTimeProvider;

        public LabourerClient(
            BaselineLabourerConfiguration configuration,
            IResourceLocker resourceLocker,
            IStoreWriterTransactionManager storeWriterTransactionManager,
            IQueue queue
        )
        {
            _configuration = configuration;
            _resourceLocker = resourceLocker;
            _storeWriterTransactionManager = storeWriterTransactionManager;
            _jobDispatcher = new JobDispatcher(storeWriterTransactionManager, queue);
            _dateTimeProvider = new DateTimeProvider();
        }

        /// <inheritdoc />
        public async Task<string> DispatchJobAsync<TJob>(CancellationToken cancellationToken = default) where TJob : IJob
        {
            var jobDefinition = new DispatchedJobDefinition
            {
                Type = typeof(TJob).AssemblyQualifiedName,
                HasParameters = false,
                Status = JobStatus.Created,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _jobDispatcher.DispatchJobAsync(jobDefinition, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> DispatchJobAsync<TParams, TJob>(
            TParams jobParameters,
            CancellationToken cancellationToken = default
        ) where TJob : IJob<TParams>
        {
            var jobDefinition = new DispatchedJobDefinition
            {
                Type = typeof(TJob).AssemblyQualifiedName,
                HasParameters = true,
                ParametersType = typeof(TParams).AssemblyQualifiedName,
                SerializedParameters = await SerializationUtils.SerializeToStringAsync(jobParameters, cancellationToken),
                Status = JobStatus.Created,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _jobDispatcher.DispatchJobAsync(jobDefinition, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> ScheduleJobAsync<TJob>(
            string cronExpression, 
            CancellationToken cancellationToken = default
        ) where TJob : IJob
        {
            await using var storeWriter = _storeWriterTransactionManager.BeginTransaction();
            
            var scheduledJobDefinition = new ScheduledJobDefinition
            {
                CronExpression = cronExpression,
                Type = typeof(TJob).AssemblyQualifiedName,
                HasParameters = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await storeWriter.CreateScheduledJobDefinitionAsync(scheduledJobDefinition, cancellationToken);
            await scheduledJobDefinition.UpdateNextRunDateAsync(storeWriter, _dateTimeProvider, cancellationToken);
            await storeWriter.CommitAsync(cancellationToken);

            return scheduledJobDefinition.Id;
        }
    }
}