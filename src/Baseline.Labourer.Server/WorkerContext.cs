using Baseline.Labourer.Internal;

namespace Baseline.Labourer.Server;

/// <summary>
/// WorkerContext provides worker-level related information and dependencies to jobs. Information that might be
/// included in the worker context includes worker-scoped dependencies such as the dispatched job store and the
/// id of the worker that is processing the request.
/// </summary>
public record WorkerContext(ServerContext ServerContext, Worker Worker)
{
    /// <summary>
    /// Gets or sets the server context that the worker is running in.
    /// </summary>
    public ServerContext ServerContext { get; set; } = ServerContext;

    /// <summary>
    /// Gets or sets the worker being ran.
    /// </summary>
    public Worker Worker { get; set; } = Worker;
}
