using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Contracts;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Server.Contracts;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server
{
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
        public IReadOnlyCollection<Type> AdditionalDispatchedJobMiddlewares { get; set; } = new List<Type>();
        
        /// <summary>
        /// Gets or sets the default retry configuration for all jobs (that are not individually configured).
        /// </summary>
        public RetryConfiguration DefaultRetryConfiguration { get; set; } = RetryConfiguration.Default;

        /// <summary>
        /// Gets or sets the job log store to be utilised within the server.
        /// </summary>
        public IJobLogStore JobLogStore { get; set; }

        /// <summary>
        /// Gets or sets the custom retries for specific job types.
        /// </summary>
        public Dictionary<Type, RetryConfiguration> JobRetryConfigurations { get; set; } = new Dictionary<Type, RetryConfiguration>();

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
        /// Gets or sets a resource locker to use.
        /// </summary>
        public IResourceLocker ResourceLocker { get; set; }

        /// <summary>
        /// Gets or sets the interval between each run of the scheduled job processor.
        /// </summary>
        public TimeSpan ScheduledJobProcessorInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets the reader to use to read the store.
        /// </summary>
        public IStoreReader StoreReader { get; set; }

        /// <summary>
        /// Gets or sets the server instance this context relates to.
        /// </summary>
        public ServerInstance ServerInstance { get; set; }

        /// <summary>
        /// Gets or sets the writer transaction manager to use to transactionally write modifications to the relevant server store.
        /// </summary>
        public IStoreWriterTransactionManager StoreWriterTransactionManager { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="CancellationTokenSource"/> used to perform a graceful shutdown of all processing
        /// tasks.
        /// </summary>
        public CancellationTokenSource ShutdownTokenSource { get; set; } = new CancellationTokenSource();

        /// <summary>
        /// Gets or sets the amount of job processing workers to run within this particular server.
        /// </summary>
        public int JobProcessingWorkersToRun { get; set; } = 1;

        public ServerContext(ServerInstance serverInstance, BaselineServerConfiguration serverConfiguration)
        {
            // validate configuration
            
            Activator = serverConfiguration.Activator;
            AdditionalDispatchedJobMiddlewares = serverConfiguration.DispatchedJobMiddlewares!;
            DefaultRetryConfiguration = serverConfiguration.DefaultRetryConfiguration;
            JobLogStore = serverConfiguration.Store!.JobLogStore;
            JobRetryConfigurations = serverConfiguration.JobRetryConfigurations;
            LoggerFactory = serverConfiguration.LoggerFactory!();
            Queue = serverConfiguration.Queue!;
            ResourceLocker = serverConfiguration.Store.ResourceLocker;
            ScheduledJobProcessorInterval = serverConfiguration.ScheduledJobProcessorInterval; 
            StoreReader = serverConfiguration.Store.StoreReader;
            ServerInstance = serverInstance;
            StoreWriterTransactionManager = serverConfiguration.Store.StoreWriterTransactionManager;
            ShutdownTokenSource = serverConfiguration.ShutdownTokenSource;
            JobProcessingWorkersToRun = serverConfiguration.JobProcessingWorkersToRun;
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
}