using System;
using System.Threading;
using System.Threading.Tasks;

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
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task<string> DispatchJobAsync(
        DispatchedJobDefinition jobDefinition, 
        CancellationToken cancellationToken
    )
    {
        await using var writer = _transactionManager.BeginTransaction();

        await writer.CreateDispatchedJobAsync(jobDefinition, cancellationToken);
        await _queue.EnqueueAsync(jobDefinition, TimeSpan.Zero, cancellationToken);

        await writer.CommitAsync(cancellationToken);

        return jobDefinition.Id;
    }
}