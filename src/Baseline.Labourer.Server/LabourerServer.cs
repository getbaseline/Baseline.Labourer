using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Contracts;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Internal.Utils;

namespace Baseline.Labourer.Server
{
    public class LabourerServer
    {
        private LabourerServer(BaselineServerConfiguration serverConfiguration)
        {
            
        }

        public async Task StartWorkersAsync(
            BaselineServerConfiguration serverConfiguration,
            IStoreWriterTransactionManager storeWriter,
            IQueue queue
        )
        {
            await using var writer = storeWriter.BeginTransaction();
            
            var serverInstance = new ServerInstance
            {
                Hostname = Dns.GetHostName(),
                Key = StringGenerationUtils.GenerateUniqueRandomString()
            };
            
            await writer.CreateServerAsync(serverInstance, CancellationToken.None);
            await writer.CommitAsync(CancellationToken.None);

            var serverContext = new ServerContext(serverInstance, serverConfiguration);

            await Task.WhenAll(
                new ServerHeartbeatWorker.ServerHeartbeatWorker(serverContext).RunAsync(),
                new ScheduledJobDispatcherWorker.ScheduledJobDispatcherWorker(serverContext).RunAsync(),
                new JobProcessorWorker.JobProcessorWorker(serverContext).RunAsync()
            );
        }
    }
}