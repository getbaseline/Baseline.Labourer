using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Contracts;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Internal.Utils;
using Baseline.Labourer.Server.Internal.JobProcessorWorker;
using Baseline.Labourer.Server.Internal.ScheduledJobDispatcherWorker;
using Baseline.Labourer.Server.Internal.ServerHeartbeatWorker;

namespace Baseline.Labourer.Server
{
    /// <summary>
    /// Entry point to running the Baseline.Labourer server and all relevant workers.
    /// </summary>
    public class LabourerServer
    {
        private readonly BaselineServerConfiguration _serverConfiguration;

        public LabourerServer(BaselineServerConfiguration serverConfiguration)
        {
            _serverConfiguration = serverConfiguration;
        }

        /// <summary>
        /// Runs the Baseline.Labourer server, booting all relevant workers and entering an infinite processing loop
        /// until the cancellation token source is called.
        /// </summary>
        public async Task RunServerAsync()
        {
            var serverInstance = await CreateServerInstanceAsync();
            var serverContext = new ServerContext(serverInstance, _serverConfiguration);

            await RunServerBootTasksAsync();
            
            await Task.WhenAll(
                new ServerHeartbeatWorker(serverContext).RunAsync(),
                new ScheduledJobDispatcherWorker(serverContext).RunAsync(),
                new JobProcessorWorker(serverContext).RunAsync()
            );
        }

        private async Task<ServerInstance> CreateServerInstanceAsync()
        {
            await using var writer = _serverConfiguration.Store!.StoreWriterTransactionManager.BeginTransaction();

            var serverInstance = new ServerInstance
            {
                Hostname = Dns.GetHostName(),
                Key = StringGenerationUtils.GenerateUniqueRandomString()
            };
            
            await writer.CreateServerAsync(serverInstance, CancellationToken.None);
            await writer.CommitAsync(CancellationToken.None);

            return serverInstance;
        }

        private Task RunServerBootTasksAsync()
        {
            return Task.CompletedTask;
        }
    }
}