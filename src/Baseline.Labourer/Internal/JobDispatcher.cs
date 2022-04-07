using System;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;

namespace Baseline.Labourer.Internal;

/// <summary>
/// JobDispatcher provides the ability to dispatch jobs and have them immediately ran.
/// </summary>
public class JobDispatcher
{
    private readonly IStoreWriterTransactionManager _transactionManager;
    private readonly IQueue _queue;

    public JobDispatcher(IStoreWriterTransactionManager transactionManager, IQueue queue)
    {
        _transactionManager = transactionManager;
        _queue = queue;
    }

    /// <summary>
    /// Immediately dispatches a job.
    /// </summary>
    /// <param name="jobDefinition">The job definition to dispatch.</param>
    public async Task<string> DispatchJobAsync(DispatchedJobDefinition jobDefinition)
    {
        await _queue.EnqueueAsync(jobDefinition, TimeSpan.Zero);

        await using var writer = _transactionManager.BeginTransaction();
        await writer.CreateDispatchedJobAsync(jobDefinition);
        await writer.CommitAsync();

        return jobDefinition.Id;
    }
}
