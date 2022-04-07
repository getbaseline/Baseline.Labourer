using System;
using System.Threading;
using Baseline.Labourer.Server;

namespace Baseline.Labourer.DependencyInjection;

/// <summary>
/// Extension methods to fluently configure the <see cref="LabourerServerBuilder" /> class.
/// </summary>
public static class LabourerServerBuilderExtensions
{
    /// <summary>
    /// Adds a dispatched job middleware of a specific type.
    /// </summary>
    /// <param name="serverBuilder">The server builder to add the middleware to.</param>
    public static LabourerServerBuilder AddDispatchedJobMiddlewareOfType<T>(
        this LabourerServerBuilder serverBuilder
    ) where T : IJobMiddleware
    {
        serverBuilder.DispatchedJobMiddlewares.Add(typeof(T));
        return serverBuilder;
    }

    /// <summary>
    /// Adds a specified retry configuration for a specified job type that does not utilise parameters.
    /// </summary>
    /// <param name="serverBuilder">The server builder to add the middleware to.</param>
    /// <param name="retryConfiguration">The retry configuration for the job type.</param>
    public static LabourerServerBuilder AddRetryConfigurationForJobType<T>(
        this LabourerServerBuilder serverBuilder,
        RetryConfiguration retryConfiguration
    )
    {
        serverBuilder.JobRetryConfigurations[typeof(T)] = retryConfiguration;
        return serverBuilder;
    }

    /// <summary>
    /// Configures the global retry configuration for all jobs.
    /// Can be overriden by utilising the <see cref="AddRetryConfigurationForJobType{T}"/> method.
    /// </summary>
    /// <param name="serverBuilder">The server builder to add the middleware to.</param>
    /// <param name="defaultRetryConfiguration">The default retry configuration.</param>
    public static LabourerServerBuilder SetRetryConfigurationForAllJobsToBe(
        this LabourerServerBuilder serverBuilder,
        RetryConfiguration defaultRetryConfiguration
    )
    {
        serverBuilder.DefaultRetryConfiguration = defaultRetryConfiguration;
        return serverBuilder;
    }

    /// <summary>
    /// Runs a defined amount of job processing workers. These workers are individual tasks that run concurrently
    /// with others to instantiate and process individual jobs.
    /// </summary>
    /// <param name="serverBuilder">The server builder to add the middleware to.</param>
    /// <param name="jobProcessingWorkersToRun">The number of job processing workers to run.</param>
    public static LabourerServerBuilder RunThisManyJobProcessingWorkers(
        this LabourerServerBuilder serverBuilder,
        int jobProcessingWorkersToRun
    )
    {
        serverBuilder.JobProcessingWorkersToRun = jobProcessingWorkersToRun;
        return serverBuilder;
    }

    /// <summary>
    /// Configures the wait between each run of the scheduled job worker (the worker that identifies jobs that need
    /// to be ran).
    /// </summary>
    /// <param name="serverBuilder">The server builder to add the middleware to.</param>
    /// <param name="delayBetweenCheckingForScheduledJobs">The delay between the checking of scheduled jobs.</param>
    /// <returns></returns>
    public static LabourerServerBuilder WaitThisLongBetweenCheckingForScheduledJobs(
        this LabourerServerBuilder serverBuilder,
        TimeSpan delayBetweenCheckingForScheduledJobs
    )
    {
        serverBuilder.ScheduledJobProcessorInterval = delayBetweenCheckingForScheduledJobs;
        return serverBuilder;
    }

    /// <summary>
    /// Configures a cancellation token source that is checked within each long running worker and used to provide
    /// a graceful shutdown.
    /// </summary>
    /// <param name="serverBuilder">A server builder instance to modify.</param>
    /// <param name="shutdownTokenSource">The cancellation token source to use.</param>
    /// <returns></returns>
    public static LabourerServerBuilder UseThisShutdownTokenSource(
        this LabourerServerBuilder serverBuilder,
        CancellationTokenSource shutdownTokenSource
    )
    {
        serverBuilder.ShutdownTokenSource = shutdownTokenSource;
        return serverBuilder;
    }
}
