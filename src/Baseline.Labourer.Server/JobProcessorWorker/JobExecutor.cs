using Baseline.Labourer.Internal.Utils;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.JobProcessorWorker;

/// <summary>
/// <see cref="JobExecutor"/> is where the magic happens for job processing.
/// </summary>
public class JobExecutor
{
    private readonly JobContext _jobContext;
    private readonly ILogger<JobExecutor> _logger;
    private readonly ILogger<JobExecutor> _jobStoredLogger;

    public JobExecutor(JobContext jobContext)
    {
        _jobContext = jobContext;
        _logger = jobContext.WorkerContext.ServerContext.LoggerFactory.CreateLogger<JobExecutor>();
        _jobStoredLogger = new JobLoggerFactory(_jobContext).CreateLogger<JobExecutor>();
    }

    /// <summary>
    /// Executes a job handling any associated tasks that need to be ran during it.
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task ExecuteJobAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(_jobContext, "Job processing started.");

        try
        {
            await using (var writer = _jobContext.WorkerContext.ServerContext.StoreWriterTransactionManager.BeginTransaction())
            {
                await _jobContext.UpdateJobStateAsync(writer, JobStatus.InProgress, cancellationToken);
                await writer.CommitAsync(cancellationToken);
            }

            await ActivateAndExecuteJobAsync(cancellationToken);

            await using (var writer = _jobContext.WorkerContext.ServerContext.StoreWriterTransactionManager.BeginTransaction())
            {
                await _jobContext.UpdateJobStateAsync(writer, JobStatus.Complete, cancellationToken);
                await writer.CommitAsync(cancellationToken);
            }

            _logger.LogInformation(_jobContext, "Job processing complete.");
        }
        catch (Exception e)
        {
            _jobStoredLogger.LogError(_jobContext, "Job failed.", e);

            if (_jobContext.JobDefinition.Retries == 3)
            {
                await FailJobDueToRetriesBeingExceededAsync(cancellationToken);
                return;
            }

            await RetryJobAsync(cancellationToken);
        }
        finally
        {
            await _jobContext.RemoveMessageFromQueueAsync(cancellationToken);
        }
    }

    private async Task ActivateAndExecuteJobAsync(CancellationToken cancellationToken)
    {
        if (_jobContext.JobDefinition.HasParameters)
        {
            var deserializedParameters = await DeserializeParametersFromContextAsync();
            await ActivateAndExecuteJobWithMethodParametersAsync(deserializedParameters, CancellationToken.None);
        }
        else
        {
            await ActivateAndExecuteJobWithMethodParametersAsync(CancellationToken.None);
        }
    }

    private async Task ActivateAndExecuteJobWithMethodParametersAsync(params object[] methodParameters)
    {
        var jobType = GetJobTypeFromContext();
        var jobInstance = ActivateJobWithDefaults(_jobContext, jobType);

        await (
            jobType
                .GetMethod(nameof(IJob.HandleAsync))!
                .Invoke(jobInstance, methodParameters) as Task
        );
    }

    private Type GetJobTypeFromContext()
    {
        return Type.GetType(_jobContext.JobDefinition.Type);
    }

    private async Task<object> DeserializeParametersFromContextAsync()
    {
        var parametersType = Type.GetType(_jobContext.JobDefinition.ParametersType);

        var deserializedParameters = await SerializationUtils.DeserializeFromStringAsync(
            _jobContext.JobDefinition.SerializedParameters,
            parametersType,
            CancellationToken.None
        );

        return deserializedParameters;
    }

    private object ActivateJobWithDefaults(JobContext jobContext, Type jobType)
    {
        var genericLogger = Activator.CreateInstance(
            typeof(Logger<>).MakeGenericType(jobType),
            new JobLoggerFactory(jobContext)
        );

        return jobContext.WorkerContext.ServerContext.Activator.ActivateJob(jobType, genericLogger);
    }

    private async Task RetryJobAsync(CancellationToken cancellationToken)
    {
        _jobStoredLogger.LogInformation(
            _jobContext,
            $"Retrying job. Attempt {_jobContext.JobDefinition.Retries + 1} of 3."
        );

        await using var writer = _jobContext.WorkerContext.ServerContext.StoreWriterTransactionManager.BeginTransaction();

        await _jobContext.UpdateJobStateAsync(writer, JobStatus.Failed, cancellationToken);
        await _jobContext.IncrementJobRetriesAsync(writer, cancellationToken);
        await _jobContext.RequeueJobAsync(cancellationToken);

        await writer.CommitAsync(cancellationToken);
    }

    private async Task FailJobDueToRetriesBeingExceededAsync(CancellationToken cancellationToken)
    {
        _jobStoredLogger.LogError(
            _jobContext,
            "Job has exceeded its maximum amount of retries. Marking job as failed."
        );

        await using var writer = _jobContext.WorkerContext.ServerContext.StoreWriterTransactionManager.BeginTransaction();

        await _jobContext.UpdateJobStateAsync(writer, JobStatus.FailedExceededMaximumRetries, cancellationToken);

        await writer.CommitAsync(cancellationToken);
    }
}
