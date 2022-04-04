using System;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Extensions;
using Baseline.Labourer.Server.Internal.JobProcessorWorker;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.Internal.ServerHeartbeatWorker;

/// <summary>
/// Continuously dispatches heartbeat notifications to the server store, ensuring it remains visible and providing
/// accurate diagnostics to consumers of the application.
/// </summary>
internal class ServerHeartbeatWorker : IWorker
{
    private readonly ServerContext _serverContext;
    private readonly ILogger<ServerHeartbeatWorker> _logger;

    public ServerHeartbeatWorker(ServerContext serverContext)
    {
        _serverContext = serverContext;
        _logger = serverContext.LoggerFactory.CreateLogger<ServerHeartbeatWorker>();
    }

    /// <inheritdoc />
    public async Task RunAsync()
    {
        try
        {
            while (true)
            {
                _logger.LogInformation(_serverContext, "Server has a heartbeat.");

                if (_serverContext.ShutdownTokenSource.IsCancellationRequested)
                {
                    _logger.LogInformation(
                        _serverContext,
                        "Shut down request received. Shutting down gracefully (hopefully)."
                    );
                    return;
                }

                await using (
                    var writer = _serverContext.Store.WriterTransactionManager.BeginTransaction()
                )
                {
                    await _serverContext.BeatAsync(writer);
                    await writer.CommitAsync();
                }

                await _serverContext.ShutdownTokenSource.WaitForTimeOrCancellationAsync(
                    TimeSpan.FromSeconds(30)
                );
            }
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
            _logger.LogError(_serverContext, "Unexpected error received. Handling.", e);
        }
    }
}
