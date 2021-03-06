using System;
using System.Threading.Tasks;
using Baseline.Labourer.Exceptions;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Contracts;
using Baseline.Labourer.Internal.Extensions;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Server.Internal.JobProcessorWorker;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.Internal.ScheduledJobDispatcherWorker;

/// <summary>
/// Queries scheduled jobs and dispatches those that need to be ran.
/// </summary>
internal class ScheduledJobDispatcherWorker : IWorker
{
    private readonly ServerContext _serverContext;
    private readonly JobDispatcher _jobDispatcher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<ScheduledJobDispatcherWorker> _logger;

    public ScheduledJobDispatcherWorker(ServerContext serverContext)
    {
        _serverContext = serverContext;
        _jobDispatcher = new JobDispatcher(
            serverContext.Store.WriterTransactionManager,
            serverContext.Queue
        );
        _dateTimeProvider = new DateTimeProvider();
        _logger = _serverContext.LoggerFactory.CreateLogger<ScheduledJobDispatcherWorker>();
    }

    public ScheduledJobDispatcherWorker(
        ServerContext serverContext,
        IDateTimeProvider dateTimeProvider
    ) : this(serverContext)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc />
    public async Task RunAsync()
    {
        _logger.LogInformation("Starting scheduled job dispatcher worker.");

        while (true)
        {
            try
            {
                if (_serverContext.ShutdownTokenSource.IsCancellationRequested)
                {
                    _logger.LogInformation(
                        _serverContext,
                        "Shutdown request received. Ending scheduled job worker."
                    );
                    return;
                }

                var beforeDate = _dateTimeProvider.UtcNow().AddSeconds(1);

                var jobsThatNeedRunning =
                    await _serverContext.Store.Reader.GetScheduledJobsDueToRunBeforeDateAsync(
                        beforeDate
                    );

                _logger.LogDebug(
                    _serverContext,
                    "Found {jobsThatNeedRunningCount} job(s) that need dispatching.",
                    jobsThatNeedRunning.Count
                );

                foreach (var job in jobsThatNeedRunning)
                {
                    try
                    {
                        await using var _ = await job.LockJobAsync(
                            _serverContext.Store.ResourceLocker,
                            TimeSpan.FromSeconds(10)
                        );

                        _logger.LogInformation(
                            _serverContext,
                            "Dispatching scheduled job {jobId}.",
                            job.Id
                        );

                        await _jobDispatcher.DispatchJobAsync(new DispatchedJobDefinition(job));

                        await using var writer =
                            _serverContext.Store.WriterTransactionManager.BeginTransaction();
                        await job.UpdateNextRunDateAsync(writer, _dateTimeProvider);
                        await job.UpdateLastRunDateAsync(writer, _dateTimeProvider);
                        await writer.CommitAsync();
                    }
                    catch (ResourceLockedException)
                    {
                        _logger.LogDebug(
                            _serverContext,
                            "Job '{jobId}' already has an active lock. Ignoring it for now.",
                            job.Id
                        );
                    }
                }

                await _serverContext.ShutdownTokenSource.WaitForTimeOrCancellationAsync(
                    _serverContext.ScheduledJobProcessorInterval
                );
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation(
                    _serverContext,
                    "Shut down request received. Shutting down gracefully (hopefully)."
                );
            }
            catch (Exception e)
            {
                _logger.LogError(_serverContext, "Unhandled exception received. Handling.", e);
            }
        }
    }
}
