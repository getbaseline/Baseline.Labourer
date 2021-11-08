using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Extensions;
using Baseline.Labourer.Server.Contracts;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.ServerHeartbeatWorker
{
    /// <summary>
    /// <see cref="ServerHeartbeatWorker" /> continuously dispatches heartbeat notifications to the server store,
    /// ensuring it remains visible and providing accurate diagnostics to consumers of the application.
    /// </summary>
    public class ServerHeartbeatWorker : IWorker
    {
        private readonly ServerContext _serverContext;
        private readonly ILogger<ServerHeartbeatWorker> _logger;

        public ServerHeartbeatWorker(ServerContext serverContext)
        {
            _serverContext = serverContext;
            _logger = serverContext.LoggerFactory.CreateLogger<ServerHeartbeatWorker>();
        }

        /// <summary>
        /// Runs the heartbeat worker.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                while (true)
                {
                    _logger.LogInformation($"{_serverContext.ServerInstance.Id} - Server has a heartbeat.");
                    
                    if (_serverContext.ShutdownTokenSource.IsCancellationRequested)
                    {
                        return;
                    }

                    await _serverContext.BeatAsync(cancellationToken);
                    await _serverContext.ShutdownTokenSource.WaitForTimeOrCancellationAsync(TimeSpan.FromSeconds(30));
                }
            }
            catch (TaskCanceledException e) when (_serverContext.IsServerOwnedCancellationToken(e.CancellationToken))
            {
                
            }
            catch (Exception e)
            {
                
            }
        }
    }
}