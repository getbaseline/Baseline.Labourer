using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Baseline.Labourer.Server;

/// <summary>
/// ServerContext provides dependencies and information related to the entire server instance.
/// </summary>
public class ServerContext
{
    /// <summary>
    /// Gets or sets the configured activator used to create instances of jobs.
    /// </summary>
    public IActivator Activator { get; set; }

    /// <summary>
    /// Gets or sets any additional middlewares that should run on top of the ones provided by the library.
    /// </summary>
    public IReadOnlyCollection<Type> AdditionalDispatchedJobMiddlewares { get; set; }
        
    /// <summary>
    /// Gets or sets the default retry configuration for all jobs (that are not individually configured).
    /// </summary>
    public RetryConfiguration DefaultRetryConfiguration { get; set; }
        
    /// <summary>
    /// Gets or sets the amount of job processing workers to run within this particular server.
    /// </summary>
    public int JobProcessingWorkersToRun { get; set; } = 1;
        
    /// <summary>
    /// Gets or sets the custom retries for specific job types.
    /// </summary>
    public Dictionary<Type, RetryConfiguration> JobRetryConfigurations { get; set; }

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
    /// Gets or sets the interval between each run of the scheduled job processor.
    /// </summary>
    public TimeSpan ScheduledJobProcessorInterval { get; set; }

    /// <summary>
    /// Gets or sets the server instance this context relates to.
    /// </summary>
    public ServerInstance ServerInstance { get; set; }

    /// <summary>
    /// Gets or sets a <see cref="CancellationTokenSource"/> used to perform a graceful shutdown of all processing
    /// tasks.
    /// </summary>
    public CancellationTokenSource ShutdownTokenSource { get; set; }

    /// <summary>
    /// Gets or sets the store to be utilised within the server.
    /// </summary>
    public IStore Store { get; set; }

    public ServerContext(ServerInstance serverInstance, BaselineLabourerServerConfiguration labourerServerConfiguration)
    {
        // validate configuration
            
        Activator = labourerServerConfiguration.Activator;
        AdditionalDispatchedJobMiddlewares = labourerServerConfiguration.DispatchedJobMiddlewares!;
        DefaultRetryConfiguration = labourerServerConfiguration.DefaultRetryConfiguration;
        JobRetryConfigurations = labourerServerConfiguration.JobRetryConfigurations;
        LoggerFactory = labourerServerConfiguration.LoggerFactory?.Invoke() ?? new NullLoggerFactory();
        Queue = labourerServerConfiguration.Queue!;
        ScheduledJobProcessorInterval = labourerServerConfiguration.ScheduledJobProcessorInterval; 
        ServerInstance = serverInstance;
        ShutdownTokenSource = labourerServerConfiguration.ShutdownTokenSource;
        Store = labourerServerConfiguration.Store!;
        JobProcessingWorkersToRun = labourerServerConfiguration.JobProcessingWorkersToRun;
    }

    /// <summary>
    /// Creates and stores a heartbeat indicating that this server is still alive.
    /// </summary>
    /// <param name="writer">A transactionized writer to use.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task BeatAsync(ITransactionalStoreWriter writer, CancellationToken cancellationToken)
    {
        await writer.CreateServerHeartbeatAsync(
            ServerInstance.Id,
            cancellationToken
        );
    }
        
    /// <summary>
    /// Identies and returns whether any additional dispatched job middlewares have been configured.
    /// </summary>
    public bool HasAdditionalDispatchedJobMiddlewares()
    {
        return AdditionalDispatchedJobMiddlewares.Count > 0;
    }

    /// <summary>
    /// Identifies and returns whether a specified cancellation token is one owned by the server (and used for things
    /// such as safe shutdowns).
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    public bool IsServerOwnedCancellationToken(CancellationToken cancellationToken)
    {
        return ShutdownTokenSource.Token == cancellationToken;
    }
}