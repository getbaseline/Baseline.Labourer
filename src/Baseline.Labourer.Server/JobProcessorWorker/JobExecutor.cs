using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Utils;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.JobProcessorWorker
{
    public class JobExecutor
    {
        private readonly JobContext _jobContext;
        private readonly ILogger<JobExecutor> _logger;

        public JobExecutor(JobContext jobContext)
        {
            _jobContext = jobContext;
            _logger = jobContext.WorkerContext.ServerContext.LoggerFactory.CreateLogger<JobExecutor>();
        }

        public async Task ExecuteJobAsync()
        {
            _logger.LogInformation(_jobContext, "Job processing started.");

            var (jobType, deserializedParameters) = await GetJobTypeAndDeserializeParametersFromContextAsync();
            var (jobLogger, jobInstance) = ActivateJobWithDefaults(_jobContext, jobType);

            await _jobContext.JobStateChanger.ChangeStateAsync(JobStatus.InProgress, CancellationToken.None);
            
            try
            {
                await (
                    jobType
                        .GetMethod(nameof(IJob<object>.HandleAsync))!
                        .Invoke(jobInstance, new[] {deserializedParameters, CancellationToken.None}) as Task
                );

                await _jobContext.JobStateChanger.ChangeStateAsync(JobStatus.Complete, CancellationToken.None);
                
                _logger.LogInformation(_jobContext, "Job processing complete.");
            }
            catch (Exception e)
            {
                jobLogger.LogError("Job failed.");

                if (_jobContext.JobDefinition.Retries == 3)
                {
                    jobLogger.LogError("Job has exceeded its maximum amount of retries. Marking job as failed.", e);
                    await _jobContext.JobStateChanger.ChangeStateAsync(
                        JobStatus.FailedExceededMaximumRetries,
                        CancellationToken.None
                    );
                    return;
                }

                jobLogger.LogInformation(
                    _jobContext, 
                    $"Retrying job. Attempt {_jobContext.JobDefinition.Retries + 1} of 3."
                );

                await _jobContext.WorkerContext.ServerContext.DispatchedJobStore.UpdateJobStateAsync(
                    _jobContext.JobDefinition.Id,
                    JobStatus.Failed,
                    null
                );

                _jobContext.JobDefinition.Retries += 1;
                
                await _jobContext.WorkerContext.ServerContext.DispatchedJobStore.UpdateJobRetriesAsync(
                    _jobContext.JobDefinition.Id,
                    _jobContext.JobDefinition.Retries,
                    CancellationToken.None
                );

                await _jobContext.WorkerContext.ServerContext.Queue.EnqueueAsync(_jobContext.JobDefinition);
            }
            finally
            {
                await _jobContext.WorkerContext.ServerContext.Queue.DeleteMessageAsync(_jobContext.OriginalMessageId);
            }
        }

        private async Task<(Type, object)> GetJobTypeAndDeserializeParametersFromContextAsync()
        {
            var parametersType = Type.GetType(_jobContext.JobDefinition.ParametersType);
            var jobType = Type.GetType(_jobContext.JobDefinition.Type);
            
            var deserializedParameters = await SerializationUtils.DeserializeFromStringAsync(
                _jobContext.JobDefinition.SerializedParameters, 
                parametersType,
                CancellationToken.None
            );

            return (jobType, deserializedParameters);
        }

        private (ILogger, object) ActivateJobWithDefaults(JobContext jobContext, Type jobType)
        {
            var genericLogger = Activator.CreateInstance(
                typeof(Logger<>).MakeGenericType(jobType), 
                new JobLoggerFactory(jobContext)
            );
            
            return (
                genericLogger as ILogger, 
                jobContext.WorkerContext.ServerContext.Activator.ActivateJob(jobType, genericLogger)
            );
        }
    }
}