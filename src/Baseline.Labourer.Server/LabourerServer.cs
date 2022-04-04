using System.Net;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Internal.Utils;
using Baseline.Labourer.Server.Internal.BootTasks;
using Baseline.Labourer.Server.Internal.JobProcessorWorker;
using Baseline.Labourer.Server.Internal.ScheduledJobDispatcherWorker;
using Baseline.Labourer.Server.Internal.ServerHeartbeatWorker;

namespace Baseline.Labourer.Server;

/// <summary>
/// Entry point to running the Baseline.Labourer server and all relevant workers.
/// </summary>
public class LabourerServer
{
    private readonly BaselineLabourerServerConfiguration _labourerServerConfiguration;

    public LabourerServer(BaselineLabourerServerConfiguration labourerServerConfiguration)
    {
        _labourerServerConfiguration = labourerServerConfiguration;
    }

    /// <summary>
    /// Runs the Baseline.Labourer server, booting all relevant workers and entering an infinite processing loop
    /// until the cancellation token source is called.
    /// </summary>
    public async Task RunServerAsync()
    {
        var serverInstance = new ServerInstance
        {
            Hostname = Dns.GetHostName(),
            Key = StringGenerationUtils.GenerateUniqueRandomString()
        };
        var serverContext = new ServerContext(serverInstance, _labourerServerConfiguration);

        await RunServerBootTasksAsync(serverContext);
        await StoreServerInstanceAsync(serverInstance);

        await Task.WhenAll(
            new ServerHeartbeatWorker(serverContext).RunAsync(),
            new ScheduledJobDispatcherWorker(
                serverContext,
                _labourerServerConfiguration.DateTimeProvider
            ).RunAsync(),
            new JobProcessorWorker(serverContext).RunAsync()
        );
    }

    private async Task StoreServerInstanceAsync(ServerInstance serverInstance)
    {
        await using var writer =
            _labourerServerConfiguration.Store!.WriterTransactionManager.BeginTransaction();

        await writer.CreateServerAsync(serverInstance);
        await writer.CommitAsync();
    }

    private async Task RunServerBootTasksAsync(ServerContext serverContext)
    {
        var bootTasks = new IBootTask[]
        {
            new BootstrapQueueBootTask(),
            new BootstrapStoreBootTask()
        };

        foreach (var bootTask in bootTasks)
        {
            await bootTask.OnBootAsync(serverContext);
        }
    }
}
