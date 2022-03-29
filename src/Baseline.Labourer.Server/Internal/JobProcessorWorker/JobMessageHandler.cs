using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.Internal;

/// <summary>
/// <see cref="JobMessageHandler"/> contains logic related to the handling of any job messages.
/// </summary>
public class JobMessageHandler
{
    private readonly WorkerContext _workerContext;
    private readonly ILogger _logger;

    public JobMessageHandler(WorkerContext workerContext)
    {
        _workerContext = workerContext;
        _logger = workerContext.ServerContext.LoggerFactory.CreateLogger<JobMessageHandler>();
    }

    /// <summary>
    /// Handles a job message, executing the job if it is applicable.
    /// </summary>
    /// <param name="job">A queued job that needs to be processed.</param>
    public async Task HandleMessageAsync(QueuedJob job)
    {
        _logger.LogDebug(
            _workerContext,
            "Handling job message with id of {messageId}.",
            job.MessageId
        );

        var jobContext = new JobContext(
            job.MessageId,
            _workerContext,
            await job.DeserializeAsync<DispatchedJobDefinition>()
        );

        try
        {
            await using var _ = await jobContext.AcquireJobLockAsync();
            await new JobExecutor(jobContext).ExecuteJobAsync();
        }
        catch (ResourceLockedException e)
        {
            _logger.LogError(jobContext, "Unable to acquire an exclusive lock of the job.", e);
        }
    }
}
