using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Exceptions;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Contracts;
using Baseline.Labourer.Internal.Extensions;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Server.Contracts;
using Baseline.Labourer.Server.JobProcessorWorker;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.ScheduledJobDispatcherWorker
{
    /// <summary>
    /// Queries scheduled jobs and dispatches those that need to be ran. 
    /// </summary>
    public class ScheduledJobDispatcherWorker : IWorker
    {
        private readonly ServerContext _serverContext;
        private readonly JobDispatcher _jobDispatcher;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<ScheduledJobDispatcherWorker> _logger;

        public ScheduledJobDispatcherWorker(ServerContext serverContext)
        {
            _serverContext = serverContext;
            _jobDispatcher = new JobDispatcher(serverContext.StoreWriterTransactionManager, serverContext.Queue);
            _dateTimeProvider = new DateTimeProvider();
            _logger = _serverContext.LoggerFactory.CreateLogger<ScheduledJobDispatcherWorker>();
        }

        public ScheduledJobDispatcherWorker(ServerContext serverContext, IDateTimeProvider dateTimeProvider) 
            : this(serverContext)
        {
            _dateTimeProvider = dateTimeProvider;
        }
        
        /// <inheritdoc />
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting scheduled job dispatcher worker.");
            
            while (true)
            {
                try
                {
                    var beforeDate = _dateTimeProvider.UtcNow().AddSeconds(1);

                    var jobsThatNeedRunning = await _serverContext.StoreReader.GetScheduledJobsDueToRunBeforeDateAsync(
                        beforeDate,
                        cancellationToken
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
                                _serverContext.ResourceLocker,
                                TimeSpan.FromSeconds(10), 
                                cancellationToken
                            );
                            
                            _logger.LogInformation(_serverContext, "Dispatching scheduled job {jobId}.", job.Id);
                        
                            await _jobDispatcher.DispatchJobAsync(new DispatchedJobDefinition(job), cancellationToken);

                            await using var writer = _serverContext.StoreWriterTransactionManager.BeginTransaction();
                            await job.UpdateNextRunDateAsync(writer, _dateTimeProvider, cancellationToken);
                            await job.UpdateLastRunDateAsync(writer, _dateTimeProvider, cancellationToken);
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
                catch (TaskCanceledException e) when (_serverContext.IsServerOwnedCancellationToken(e.CancellationToken))
                {
                    _logger.LogInformation(_serverContext, "Shutdown request received. Ending scheduled job worker.");
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogError(_serverContext, "Unhandled exception received. Handling.", e);
                }
            }
        }
    }
}