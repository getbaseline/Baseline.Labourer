using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Models;

namespace Baseline.Labourer.Server.JobProcessorWorker
{
    /// <summary>
    /// WorkerContext provides worker-level related information and dependencies to jobs. Information that might be
    /// included in the worker context includes worker-scoped dependencies such as the dispatched job store and the
    /// id of the worker that is processing the request.
    /// </summary>
    public class WorkerContext
    {
        /// <summary>
        /// Gets or sets the server context that the worker is running in.
        /// </summary>
        public ServerContext ServerContext { get; set; }

        /// <summary>
        /// Gets or sets the worker being ran.
        /// </summary>
        public Worker Worker { get; set; }
    }
}