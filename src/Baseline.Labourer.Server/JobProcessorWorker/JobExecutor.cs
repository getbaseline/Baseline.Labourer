using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Utils;
using Baseline.Labourer.Server.Middleware;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.JobProcessorWorker
{
    /// <summary>
    /// <see cref="JobExecutor"/> is where the magic happens for job processing.
    /// </summary>
    public class JobExecutor
    {
        private readonly JobContext _jobContext;
        private readonly JobMiddlewareRunner _jobMiddlewareRunner;
        private readonly ILogger<JobExecutor> _logger;

        public JobExecutor(JobContext jobContext)
        {
            _jobContext = jobContext;
            _jobMiddlewareRunner = new JobMiddlewareRunner(jobContext.WorkerContext.ServerContext);
            _logger = jobContext.WorkerContext.ServerContext.LoggerFactory.CreateLogger<JobExecutor>();
        }

        /// <summary>
        /// Executes a job handling any associated tasks that need to be ran during it.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task ExecuteJobAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(_jobContext, "Job processing started.");
                await _jobMiddlewareRunner.JobStartedAsync(_jobContext, cancellationToken);

                await ActivateAndExecuteJobAsync(cancellationToken);

                _logger.LogInformation(_jobContext, "Job processing complete.");
                await _jobMiddlewareRunner.JobCompletedAsync(_jobContext, cancellationToken);
            }
            catch (Exception e)
            {
                await _jobMiddlewareRunner.JobFailedAsync(_jobContext, e, cancellationToken);
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
    }
}