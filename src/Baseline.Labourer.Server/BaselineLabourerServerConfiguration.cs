using System;
using System.Collections.Generic;
using System.Threading;
using Baseline.Labourer.Internal;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server;

/// <summary>
/// A configuration object used to configure the Baseline.Labourer server components.
/// </summary>
public class BaselineLabourerServerConfiguration
{
    /// <summary>
    /// Gets or sets the activator used to create instances of Baseline.Labourer components such as jobs, middlewares
    /// etc.
    /// </summary>
    public IActivator Activator { get; set; } = new DefaultActivator();

    /// <summary>
    /// Gets or sets the date time provider to use. Normally won't need to be changed.
    /// </summary>
    public IDateTimeProvider DateTimeProvider { get; set; } = new DateTimeProvider();

    /// <summary>
    /// Gets or sets the default amount of retries that apply to a job. Defaults to 3.
    /// </summary>
    public RetryConfiguration DefaultRetryConfiguration { get; set; } = RetryConfiguration.Default;

    /// <summary>
    /// Gets or sets the middlewares used for dispatched jobs.
    /// </summary>
    public IReadOnlyCollection<Type> DispatchedJobMiddlewares { get; set; } = new List<Type>();

    /// <summary>
    /// Gets or sets the delegate used to receive an <see cref="ILoggerFactory"/> instance which is used throughout
    /// the server and related projects.
    /// </summary>
    public Func<ILoggerFactory>? LoggerFactory { get; set; }

    /// <summary>
    /// Gets or sets the number of job processing workers to run.
    /// </summary>
    public int JobProcessingWorkersToRun { get; set; } = 20;

    /// <summary>
    /// Gets or sets the custom retries for specific job types.
    /// </summary>
    public Dictionary<Type, RetryConfiguration> JobRetryConfigurations { get; set; } = new();

    /// <summary>
    /// Gets or sets the queue provider to use.
    /// </summary>
    public IQueue? Queue { get; set; }

    /// <summary>
    /// Gets or sets the interval between each run of the scheduled job processor.
    /// </summary>
    public TimeSpan ScheduledJobProcessorInterval { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Gets or sets the store to use.
    /// </summary>
    public IStore? Store { get; set; }

    /// <summary>
    /// Gets a cancellation token source used to gracefully shutdown workers.
    /// </summary>
    public CancellationTokenSource ShutdownTokenSource { get; set; } =
        new CancellationTokenSource();
}
