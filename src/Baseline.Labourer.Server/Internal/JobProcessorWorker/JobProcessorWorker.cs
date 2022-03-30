using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.Internal;

/// <summary>
/// A worker that processes jobs that need to be ran.
/// </summary>
internal class JobProcessorWorker : IWorker
{
    private readonly ServerContext _serverContext;
    private readonly ILogger<JobProcessorWorker> _logger;

    public JobProcessorWorker(ServerContext serverContext)
    {
        _serverContext = serverContext;
        _logger = serverContext.LoggerFactory.CreateLogger<JobProcessorWorker>();
    }

    /// <inheritdoc />
    public async Task RunAsync()
    {
        _logger.LogInformation(
            _serverContext,
            "Starting job processing tasks and booting the guinea pig treadmills."
        );

        var processingTasks = Enumerable
            .Range(1, _serverContext.JobProcessingWorkersToRun)
            .Select(async _ => await RunSingleWorkerAsync());

        await Task.WhenAll(processingTasks);

        _logger.LogInformation(_serverContext, "Finished job processing tasks.");
    }

    private async Task RunSingleWorkerAsync()
    {
        var worker = new Worker
        {
            Id = StringGenerationUtils.GenerateUniqueRandomString(),
            ServerInstanceId = _serverContext.ServerInstance.Id
        };

        await using (var writer = _serverContext.Store.WriterTransactionManager.BeginTransaction())
        {
            await writer.CreateWorkerAsync(worker);
            await writer.CommitAsync();
        }

        var workerContext = new WorkerContext(_serverContext, worker);

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
                    _logger.LogInformation(
                        _serverContext,
                        "Shut down request received. Shutting down gracefully (hopefully)."
                    );
                    return;
                }

                var dequeuedMessage = await _serverContext.Queue.DequeueAsync();
                if (dequeuedMessage == null)
                {
                    // If the queue does not support long polling, then we want to implement some pause of our
                    // own so we don't continuously loop and explode CPUs!
                    if (!_serverContext.Queue.SupportsLongPolling)
                    {
                        await _serverContext.ShutdownTokenSource.WaitForTimeOrCancellationAsync(
                            TimeSpan.FromSeconds(1)
                        );
                    }

                    continue;
                }

                await new JobMessageHandler(workerContext).HandleMessageAsync(dequeuedMessage);
            }
        }
        catch (TaskCanceledException e)
        {
            _logger.LogInformation(
                workerContext,
                "Shut down request received. Shutting down gracefully (hopefully)."
            );
        }
        catch (Exception e)
        {
            _logger.LogError(workerContext, $"Unexpected error received. Handling. {e.Message}", e);
        }
    }
}
