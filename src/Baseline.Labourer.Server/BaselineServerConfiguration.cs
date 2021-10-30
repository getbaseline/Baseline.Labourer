using System.Threading;

namespace Baseline.Labourer.Server
{
    /// <summary>
    /// A configuration object used to configure the Baseline.Labourer server components.
    /// </summary>
    public class BaselineServerConfiguration
    {
        /// <summary>
        /// Gets or sets the number of job processor tasks to run within this instance of the Baseline.Labourer server.
        /// </summary>
        public int JobProcessorTasksToRun { get; set; } = 1;

        /// <summary>
        /// Gets a cancellation token source used to gracefully shutdown workers.
        /// </summary>
        public CancellationTokenSource ShutdownTokenSource { get; set; } = new CancellationTokenSource();
    }
}