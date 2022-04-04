using System;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Contracts;
using Baseline.Labourer.Internal.Extensions;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Internal.Utils;

namespace Baseline.Labourer;

/// <summary>
/// LabourerClient is the default (and officially only) implementation of the <see cref="ILabourerClient"/>
/// interface.
/// </summary>
public class LabourerClient : ILabourerClient
{
    private readonly BaselineLabourerClientConfiguration _clientConfiguration;
    private readonly JobDispatcher _jobDispatcher;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LabourerClient(BaselineLabourerClientConfiguration clientConfiguration)
    {
        _clientConfiguration = clientConfiguration;
        _jobDispatcher = new JobDispatcher(
            _clientConfiguration.Store.WriterTransactionManager,
            _clientConfiguration.Queue
        );
        _dateTimeProvider = _clientConfiguration.DateTimeProvider;
    }

    /// <inheritdoc />
    public async Task<string> CreateOrUpdateScheduledJobAsync<TJob>(
        string nameOrId,
        string cronExpression
    ) where TJob : IJob
    {
        return await InternalCreateOrUpdatedScheduledJobAsync<object, TJob>(
            nameOrId,
            cronExpression,
            null
        );
    }

    /// <inheritdoc />
    public async Task<string> CreateOrUpdateScheduledJobAsync<TParams, TJob>(
        string nameOrId,
        string cronExpression,
        TParams jobParameters
    )
        where TJob : IJob<TParams>
        where TParams : class
    {
        return await InternalCreateOrUpdatedScheduledJobAsync<TParams, TJob>(
            nameOrId,
            cronExpression,
            jobParameters
        );
    }

    /// <inheritdoc />
    public async Task DeleteScheduledJobAsync(string nameOrId)
    {
        await using var _ = await _clientConfiguration.Store.ResourceLocker.LockResourceAsync(
            nameOrId.AsNormalizedScheduledJobId(),
            TimeSpan.FromSeconds(10)
        );

        await using var storeWriter =
            _clientConfiguration.Store.WriterTransactionManager.BeginTransaction();
        await storeWriter.DeleteScheduledJobAsync(nameOrId.AsNormalizedScheduledJobId());
        await storeWriter.CommitAsync();
    }

    /// <inheritdoc />
    public async Task<string> DispatchJobAsync<TJob>() where TJob : IJob
    {
        return await InternalDispatchJobAsync<object, TJob>(null);
    }

    /// <inheritdoc />
    public async Task<string> DispatchJobAsync<TParams, TJob>(TParams jobParameters)
        where TJob : IJob<TParams>
        where TParams : class
    {
        return await InternalDispatchJobAsync<TParams, TJob>(jobParameters);
    }

    private async Task<string> InternalDispatchJobAsync<TParams, TJob>(TParams? jobParameters)
        where TParams : class
    {
        var jobDefinition = new DispatchedJobDefinition
        {
            Type = typeof(TJob).AssemblyQualifiedName!,
            HasParameters = jobParameters != null,
            ParametersType = jobParameters != null ? GetParametersType<TParams>() : null,
            SerializedParameters =
                jobParameters != null
                    ? await SerializationUtils.SerializeToStringAsync(jobParameters)
                    : null,
            Status = JobStatus.Created,
            CreatedAt = _dateTimeProvider.UtcNow(),
            UpdatedAt = _dateTimeProvider.UtcNow()
        };

        return await _jobDispatcher.DispatchJobAsync(jobDefinition);
    }

    private async Task<string> InternalCreateOrUpdatedScheduledJobAsync<TParams, TJob>(
        string name,
        string cronExpression,
        TParams? jobParameters
    ) where TParams : class
    {
        var scheduledJobDefinition = new ScheduledJobDefinition
        {
            CronExpression = cronExpression,
            Type = typeof(TJob).AssemblyQualifiedName!,
            HasParameters = jobParameters != null,
            ParametersType = jobParameters != null ? GetParametersType<TParams>() : null,
            Name = name,
            SerializedParameters =
                jobParameters != null
                    ? await SerializationUtils.SerializeToStringAsync(jobParameters)
                    : null,
            CreatedAt = _dateTimeProvider.UtcNow(),
            UpdatedAt = _dateTimeProvider.UtcNow()
        };

        await using var _ = await _clientConfiguration.Store.ResourceLocker.LockResourceAsync(
            scheduledJobDefinition.Id,
            TimeSpan.FromSeconds(10)
        );

        await using var storeWriter =
            _clientConfiguration.Store.WriterTransactionManager.BeginTransaction();
        await storeWriter.CreateOrUpdateScheduledJobAsync(scheduledJobDefinition);
        await scheduledJobDefinition.UpdateNextRunDateAsync(storeWriter, _dateTimeProvider);
        await storeWriter.CommitAsync();

        return scheduledJobDefinition.Id;
    }

    private string GetParametersType<TType>() => typeof(TType).AssemblyQualifiedName!;
}
