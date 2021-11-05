using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Utils;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.JobProcessorWorker
{
    /// <summary>
    /// JobProcessor contains the logic behind the processing of jobs. 
    /// </summary>
    public class JobProcessor
    {
        private readonly JobContext _jobContext;
        private readonly ILogger _logger;

        public JobProcessor(JobContext jobContext)
        {
            _jobContext = jobContext;
            _logger = jobContext.WorkerContext.ServerContext.LoggerFactory.CreateLogger<JobProcessor>();
        }

        /// <summary>
        /// Processes a job, managing its state and any associated dependencies.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task ProcessJobAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(_jobContext, "Job processing started.");
            
            var parametersType = Type.GetType(_jobContext.JobDefinition.ParametersType);
            var jobType = Type.GetType(_jobContext.JobDefinition.Type);
            
            // TODO - validate neither of these being null ^

            var deserializedParameters = await SerializationUtils.DeserializeFromStringAsync(
                _jobContext.JobDefinition.SerializedParameters, 
                parametersType,
                cancellationToken
            );
            
            var jobInstance = ActivateJobWithDefaults(jobType);
            
            await _jobContext.JobStateChanger.ChangeStateAsync(JobStatus.InProgress, cancellationToken);
            
            var task = jobType
                .GetMethod(nameof(IJob<object>.HandleAsync))!
                .Invoke(jobInstance, new[] { deserializedParameters, CancellationToken.None }) as Task;
            await task;

            await _jobContext.JobStateChanger.ChangeStateAsync(JobStatus.Complete, cancellationToken);
            
            _logger.LogInformation(_jobContext, "Job processing complete.");
        }

        private object ActivateJobWithDefaults(Type jobType)
        {
            var genericLogger = Activator.CreateInstance(
                typeof(Logger<>).MakeGenericType(jobType), 
                new JobLoggerFactory(_jobContext)
            );
            
            return _jobContext.WorkerContext.ServerContext.Activator.ActivateJob(jobType, genericLogger);
        }
    }
}