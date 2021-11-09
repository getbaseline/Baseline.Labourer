namespace Baseline.Labourer;

/// <summary>
/// IServerStore is a contract that defines what server stores must implement. Server stores manage all aspects
/// of servers and their associated workers.
/// </summary>
public interface IServerStore
{
    /// <summary>
    /// Creates and persists a server registration.
    /// </summary>
    /// <param name="serverInstance">The server to persist.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<ServerInstance> CreateServerAsync(ServerInstance serverInstance, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a heartbeat for a server.
    /// </summary>
    /// <param name="serverId">The server to create a heartbeat for.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task CreateServerHeartbeatAsync(string serverId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates and persists a worker registration.
    /// </summary>
    /// <param name="worker">The worker to persist.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<Worker> CreateWorkerAsync(Worker worker, CancellationToken cancellationToken);
}
