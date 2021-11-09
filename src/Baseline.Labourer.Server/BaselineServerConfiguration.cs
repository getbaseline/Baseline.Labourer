using Baseline.Labourer.Server.Contracts;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server;

/// <summary>
/// A configuration object used to configure the Baseline.Labourer server components.
/// </summary>
public class BaselineServerConfiguration
{
    /// <summary>
    /// Gets or sets the activator used to create instances of job definitions.
    /// </summary>
    public IJobActivator JobActivator { get; set; } = new DefaultJobActivator();

    /// <summary>
    /// Gets or sets the number of job processor tasks to run within this instance of the Baseline.Labourer server.
    /// </summary>
    public int JobProcessorTasksToRun { get; set; } = 1;

    /// <summary>
    /// Gets or sets the delegate used to receive an <see cref="ILoggerFactory"/> instance which is used throughout
    /// the server and related projects.
    /// </summary>
    public Func<ILoggerFactory> LoggerFactory { get; set; }

    /// <summary>
    /// Gets a cancellation token source used to gracefully shutdown workers.
    /// </summary>
    public CancellationTokenSource ShutdownTokenSource { get; set; } = new CancellationTokenSource();
}
