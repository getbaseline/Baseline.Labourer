using Baseline.Labourer.Server.Contracts;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server;

/// <summary>
/// ServerContext provides dependencies and information related to the entire server instance.
/// </summary>
public class ServerContext
{
    /// <summary>
    /// Gets or sets the configured activator used to create instances of jobs.
    /// </summary>
    public IJobActivator Activator { get; set; }

    /// <summary>
    /// Gets or sets the dispatched job store to be utilised within the server.
    /// </summary>
    public IDispatchedJobStore DispatchedJobStore { get; set; }

    /// <summary>
    /// Gets or sets an optional logger factory instance to use to log messages to destinations configured by the
    /// user of the library.
    /// </summary>
    public ILoggerFactory LoggerFactory { get; set; }

    /// <summary>
    /// Gets or sets the queue instance to be utilised within the server.
    /// </summary>
    public IQueue Queue { get; set; }

    /// <summary>
    /// Gets or sets the server instance this context relates to.
    /// </summary>
    public ServerInstance ServerInstance { get; set; }

    /// <summary>
    /// Gets or sets the server store to be utilised within the server.
    /// </summary>
    public IServerStore ServerStore { get; set; }

    /// <summary>
    /// Gets or sets a <see cref="CancellationTokenSource"/> used to perform a graceful shutdown of all processing
    /// tasks.
    /// </summary>
    public CancellationTokenSource ShutdownTokenSource { get; set; } = new CancellationTokenSource();

    /// <summary>
    /// Gets or sets the amount of workers to run within this particular server.
    /// </summary>
    public int WorkersToRun { get; set; } = 1;

    /// <summary>
    /// Identifies and returns whether a specified cancellation token is one owned by the server (and used for things
    /// such as safe shutdowns).
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    public bool IsServerOwnedCancellationToken(CancellationToken cancellationToken)
    {
        return ShutdownTokenSource.Token == cancellationToken;
    }

    /// <summary>
    /// Creates and stores a heartbeat indicating that this server is still alive.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task BeatAsync(CancellationToken cancellationToken)
    {
        await ServerStore.CreateServerHeartbeatAsync(
            ServerInstance.Id,
            CancellationToken.None
        );
    }
}
