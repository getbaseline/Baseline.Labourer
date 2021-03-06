using System;
using System.Collections.Generic;
using System.Threading;
using Baseline.Labourer.Server;

namespace Baseline.Labourer.DependencyInjection;

/// <summary>
/// An interim builder object used to fluently configure the Baseline.Labourer server.
/// </summary>
public class LabourerServerBuilder
{
    /// <summary>
    /// Gets or sets the default amount of retries that apply to a job. Defaults to 3.
    /// </summary>
    internal RetryConfiguration DefaultRetryConfiguration { get; set; } =
        RetryConfiguration.Default;

    /// <summary>
    /// Gets or sets the cancellation source to use to gracefully shut down the server.
    /// </summary>
    internal CancellationTokenSource ShutdownTokenSource { get; set; } =
        new();

    /// <summary>
    /// Gets or sets the middlewares used for dispatched jobs.
    /// </summary>
    internal List<Type> DispatchedJobMiddlewares { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of job processing workers to run.
    /// </summary>
    internal int JobProcessingWorkersToRun { get; set; } = 20;

    /// <summary>
    /// Gets or sets the custom retries for specific job types.
    /// </summary>
    internal Dictionary<Type, RetryConfiguration> JobRetryConfigurations { get; set; } =
        new();

    /// <summary>
    /// Gets or sets the interval between each run of the scheduled job processor.
    /// </summary>
    internal TimeSpan ScheduledJobProcessorInterval { get; set; } = TimeSpan.FromSeconds(60);
}
