using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Exceptions;
using Baseline.Labourer.Internal.Models;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.JobProcessorWorker
{
    /// <summary>
    /// <see cref="JobMessageHandler"/> contains logic related to the handling of any job messages.
    /// </summary>
    public class JobMessageHandler
    {
        private readonly WorkerContext _workerContext;
        private readonly ILogger _logger;

        public JobMessageHandler(WorkerContext workerContext)
        {
            _workerContext = workerContext;
            _logger = workerContext.ServerContext.LoggerFactory.CreateLogger<JobMessageHandler>();
        }

        /// <summary>
        /// Handles a job message, executing the job if it is applicable.
        /// </summary>
        /// <param name="job">A queued job that needs to be processed.</param>
        /// <param name="cancellationToken"></param>
        public async Task HandleMessageAsync(QueuedJob job, CancellationToken cancellationToken)
        {
            _logger.LogDebug(_workerContext, "Handling job message with id of {messageId}.", job.MessageId);

            var jobContext = new JobContext
            {
                OriginalMessageId = job.MessageId,
                JobDefinition = await job.DeserializeAsync<DispatchedJobDefinition>(cancellationToken),
                WorkerContext = _workerContext
            };

            try
            {
                await using var _ = await jobContext.AcquireJobLockAsync(cancellationToken);
                await new JobExecutor(jobContext).ExecuteJobAsync(cancellationToken);
            }
            catch (ResourceLockedException e)
            {
                _logger.LogError(jobContext, "Unable to acquire an exclusive lock of the job.", e);
            }
        }
    }
}