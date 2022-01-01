using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Contracts;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Contracts;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Internal.Utils;

namespace Baseline.Labourer
{
    /// <summary>
    /// LabourerClient is the default (and officially only) implementation of the <see cref="ILabourerClient"/>
    /// interface.
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
            return await InternalDispatchJobAsync<object, TJob>(null, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> DispatchJobAsync<TParams, TJob>(
            TParams jobParameters,
            CancellationToken cancellationToken = default
        ) where TJob : IJob<TParams>
        {
            return await InternalDispatchJobAsync<TParams, TJob>(jobParameters, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> CreateOrUpdateScheduledJobAsync<TJob>(
            string name,
            string cronExpression, 
            CancellationToken cancellationToken = default
        ) where TJob : IJob
        {
            return await InternalCreateOrUpdatedScheduledJobAsync<object, TJob>(name, cronExpression, null, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> CreateOrUpdateScheduledJobAsync<TParams, TJob>(
            string name,
            string cronExpression, 
            TParams jobParameters,
            CancellationToken cancellationToken = default
        ) where TJob : IJob<TParams>
        {
            return await InternalCreateOrUpdatedScheduledJobAsync<TParams, TJob>(
                name, 
                cronExpression, 
                jobParameters, 
                cancellationToken
            );
        }

        private async Task<string> InternalDispatchJobAsync<TParams, TJob>(
            TParams jobParameters,
            CancellationToken cancellationToken
        )
        {
            var jobDefinition = new DispatchedJobDefinition
            {
                Type = typeof(TJob).AssemblyQualifiedName,
                HasParameters = jobParameters != null,
                ParametersType = jobParameters != null ? GetParametersType<TParams>() : null,
                SerializedParameters = jobParameters != null ?
                    await SerializationUtils.SerializeToStringAsync(jobParameters, cancellationToken) :
                    null,
                Status = JobStatus.Created,
                CreatedAt = _dateTimeProvider.UtcNow(),
                UpdatedAt = _dateTimeProvider.UtcNow()
            };

            return await _jobDispatcher.DispatchJobAsync(jobDefinition, cancellationToken);
        }

        private async Task<string> InternalCreateOrUpdatedScheduledJobAsync<TParams, TJob>(
            string name,
            string cronExpression,  
            TParams jobParameters,
            CancellationToken cancellationToken = default
        ) 
        {
            await using var storeWriter = _storeWriterTransactionManager.BeginTransaction();
            
            var scheduledJobDefinition = new ScheduledJobDefinition
            {
                CronExpression = cronExpression,
                Type = typeof(TJob).AssemblyQualifiedName,
                HasParameters = jobParameters != null,
                ParametersType = jobParameters != null ? GetParametersType<TParams>() : null,
                Name = name,
                SerializedParameters = jobParameters != null ?
                    await SerializationUtils.SerializeToStringAsync(jobParameters, cancellationToken) :
                    null,
                CreatedAt = _dateTimeProvider.UtcNow(),
                UpdatedAt = _dateTimeProvider.UtcNow()
            };

            await storeWriter.CreateOrUpdateScheduledJobDefinitionAsync(scheduledJobDefinition, cancellationToken);
            await scheduledJobDefinition.UpdateNextRunDateAsync(storeWriter, _dateTimeProvider, cancellationToken);
            await storeWriter.CommitAsync(cancellationToken);

            return scheduledJobDefinition.Id;
        }

        private string GetParametersType<TType>() => typeof(TType).AssemblyQualifiedName;
    }
}