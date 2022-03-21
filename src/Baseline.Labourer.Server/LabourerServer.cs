using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Server.Internal;

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
        var serverInstance = await CreateServerInstanceAsync();
        var serverContext = new ServerContext(serverInstance, _labourerServerConfiguration);

        await RunServerBootTasksAsync(serverContext);
            
        await Task.WhenAll(
            new ServerHeartbeatWorker(serverContext).RunAsync(),
            new ScheduledJobDispatcherWorker(serverContext, _labourerServerConfiguration.DateTimeProvider).RunAsync(),
            new JobProcessorWorker(serverContext).RunAsync()
        );
    }

    private async Task<ServerInstance> CreateServerInstanceAsync()
    {
        await using var writer = _labourerServerConfiguration.Store!.WriterTransactionManager.BeginTransaction();

        var serverInstance = new ServerInstance
        {
            Hostname = Dns.GetHostName(),
            Key = StringGenerationUtils.GenerateUniqueRandomString()
        };
            
        await writer.CreateServerAsync(serverInstance, CancellationToken.None);
        await writer.CommitAsync(CancellationToken.None);

        return serverInstance;
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