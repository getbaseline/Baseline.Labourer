﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Utils;
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
            var deserializedJobDefinition = await SerializationUtils.DeserializeFromStringAsync<DispatchedJobDefinition>(
                job.SerializedDefinition,
                cancellationToken
            );
                    
            var jobContext = new JobContext
            {
                JobDefinition = deserializedJobDefinition,
                WorkerContext = _workerContext,
                JobStateChanger = new JobStateChanger(
                    deserializedJobDefinition.Id,
                    _workerContext.ServerContext.DispatchedJobStore
                )
            };

            await new JobExecutor(jobContext).ExecuteJobAsync();
        }
    }
}