using System;
using Baseline.Labourer.Server;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer;

/// <summary>
/// An interim class used to build up the settings required to instantiate the Baseline.Labourer client and
/// server.
/// </summary>
public class LabourerBuilder
{
    /// <summary>
    /// Gets or sets the logger factory to use.
    /// </summary>
    internal Func<ILoggerFactory>? LoggerFactory { get; set; }

    /// <summary>
    /// Gets or sets the queue to use.
    /// </summary>
    public IQueue? Queue { get; set; }

    /// <summary>
    /// Gets or sets the store to use.
    /// </summary>
    public IStore? Store { get; set; }

    /// <summary>
    /// Gets or sets the client builder to use.
    /// </summary>
    internal LabourerClientBuilder ClientBuilder { get; set; } = new();

    /// <summary>
    /// Gets or sets the server builder instance to use.
    /// </summary>
    internal LabourerServerBuilder ServerBuilder { get; set; } = new();

    /// <summary>
    /// Converts the <see cref="LabourerBuilder"/> instance into an instance of the
    /// <see cref="BaselineLabourerClientConfiguration"/> class that can be used to instantiate the client.
    /// </summary>
    public BaselineLabourerClientConfiguration ToClientConfiguration()
    {
        ArgumentNullException.ThrowIfNull(Queue);
        ArgumentNullException.ThrowIfNull(Store);

        return new BaselineLabourerClientConfiguration
        {
            LoggerFactory = LoggerFactory,
            Store = Store,
            Queue = Queue
        };
    }

    /// <summary>
    /// Converts the <see cref="LabourerBuilder"/> instance into an instance of the
    /// <see cref="BaselineLabourerServerConfiguration"/> class that can be used to instantiate the server.
    /// </summary>
    /// <param name="serviceProvider">A service provider instance.</param>
    public BaselineLabourerServerConfiguration ToServerConfiguration(
        IServiceProvider serviceProvider
    )
    {
        ArgumentNullException.ThrowIfNull(Queue);
        ArgumentNullException.ThrowIfNull(Store);

        return new BaselineLabourerServerConfiguration
        {
            Activator = new DependencyInjectionActivator(serviceProvider),
            ShutdownTokenSource = ServerBuilder.ShutdownTokenSource,
            Queue = Queue,
            Store = Store,
            LoggerFactory = LoggerFactory,
            DefaultRetryConfiguration = ServerBuilder.DefaultRetryConfiguration,
            JobRetryConfigurations = ServerBuilder.JobRetryConfigurations,
            DispatchedJobMiddlewares = ServerBuilder.DispatchedJobMiddlewares,
            ScheduledJobProcessorInterval = ServerBuilder.ScheduledJobProcessorInterval,
            JobProcessingWorkersToRun = ServerBuilder.JobProcessingWorkersToRun
        };
    }
}
