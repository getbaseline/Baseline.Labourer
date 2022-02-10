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
    public class LabourerServer
    {
        private readonly BaselineServerConfiguration _serverConfiguration;

        public LabourerServer(BaselineServerConfiguration serverConfiguration)
        {
            _serverConfiguration = serverConfiguration;
        }

        public async Task RunServerAsync()
        {
            await using var writer = _serverConfiguration.Store!.StoreWriterTransactionManager.BeginTransaction();
            
            var serverInstance = new ServerInstance
            {
                Hostname = Dns.GetHostName(),
                Key = StringGenerationUtils.GenerateUniqueRandomString()
            };
            
            await writer.CreateServerAsync(serverInstance, CancellationToken.None);
            await writer.CommitAsync(CancellationToken.None);

            var serverContext = new ServerContext(serverInstance, _serverConfiguration);

            await Task.WhenAll(
                new ServerHeartbeatWorker(serverContext).RunAsync(),
                new ScheduledJobDispatcherWorker(serverContext).RunAsync(),
                new JobProcessorWorker(serverContext).RunAsync()
            );
        }
    }
}