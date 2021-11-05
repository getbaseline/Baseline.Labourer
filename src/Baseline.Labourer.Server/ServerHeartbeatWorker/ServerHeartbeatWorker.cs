using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Extensions;
using Baseline.Labourer.Server.Contracts;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.ServerHeartbeatWorker
{
    public class ServerHeartbeatWorker : IWorker
    {
        private readonly ServerContext _serverContext;
        private readonly ILogger<ServerHeartbeatWorker> _logger;

        public ServerHeartbeatWorker(ServerContext serverContext)
        {
            _serverContext = serverContext;
            _logger = serverContext.LoggerFactory.CreateLogger<ServerHeartbeatWorker>();
        }

        public async Task RunAsync()
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

                    await _serverContext.ServerStore.CreateServerHeartbeat(
                        _serverContext.ServerInstance.Id,
                        CancellationToken.None
                    );

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