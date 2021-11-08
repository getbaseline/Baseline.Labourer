﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Utils;
using Baseline.Labourer.Server.Contracts;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.JobProcessorWorker
{
    /// <summary>
    /// A worker that processes jobs that need to be ran.
    /// </summary>
    public class JobProcessorWorker : IWorker
    {
        private readonly ServerContext _serverContext;
        private readonly ILogger<JobProcessorWorker> _logger;

        public JobProcessorWorker(
            ServerContext serverContext
        )
        {
            _serverContext = serverContext;
            _logger = serverContext.LoggerFactory.CreateLogger<JobProcessorWorker>();
        }

        /// <summary>
        /// Boots and runs the job processing worker instances as a long running task.
        /// </summary>
        public async Task RunAsync()
        {
            _logger.LogInformation(_serverContext, "Starting job processing tasks and booting the guinea pig treadmills.");
            
            var processingTasks = Enumerable
                .Range(1, _serverContext.WorkersToRun)
                .Select(async _ => await RunSingleWorkerAsync());
            
            await Task.WhenAll(processingTasks);
            
            _logger.LogInformation(_serverContext, "Finished job processing tasks.");
        }

        private async Task RunSingleWorkerAsync()
        {
            var worker = await _serverContext.ServerStore.CreateWorkerAsync(
                new Worker
                {
                    Id = StringGenerationUtils.GenerateUniqueRandomString(),
                    ServerInstanceId = _serverContext.ServerInstance.Id
                },
                CancellationToken.None
            );

            var workerContext = new WorkerContext { ServerContext = _serverContext, Worker = worker };

            await ProcessJobsAsync(workerContext);
        }

        private async Task ProcessJobsAsync(WorkerContext workerContext)
        {
            _logger.LogDebug(workerContext, "Booting worker and entering infinite processing loop.");
            
            try
            {
                while (true)
                {
                    if (_serverContext.ShutdownTokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                    
                    var dequeuedMessage = await _serverContext.Queue.DequeueAsync(_serverContext.ShutdownTokenSource.Token);
                    if (dequeuedMessage == null)
                    {
                        continue;
                    }

                    await new JobMessageHandler(workerContext).HandleMessageAsync(dequeuedMessage, CancellationToken.None);
                }
            }
            catch (TaskCanceledException e) when (_serverContext.IsServerOwnedCancellationToken(e.CancellationToken))
            {
                _logger.LogInformation(workerContext, "Shut down request received. Shutting down gracefully (hopefully).");
            }
            catch (Exception e)
            {
                _logger.LogError(workerContext, "Unexpected error received. Handling.", e);
            }
        }
    }
}