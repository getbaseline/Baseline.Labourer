using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Store.Memory;

/// <summary>
/// <see cref="MemoryJobStore"/> is a job store that persists its stored entities in memory. Created entities and any changes are lost
/// once the memory is cleared.
/// </summary>
public class MemoryJobStore : IDispatchedJobStore
{
    protected readonly List<DispatchedJobDefinition> DispatchedJobs = new List<DispatchedJobDefinition>();
    protected readonly List<MemoryLogEntry> LogEntries = new List<MemoryLogEntry>();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

    /// <inheritdoc />
    public void LogEntryForJob(string jobId, LogLevel logLevel, string message, Exception? exception)
    {
        LogEntries.Add(new MemoryLogEntry
        {
            JobId = jobId,
            LogLevel = logLevel,
            Message = message,
            Exception = exception
        });
    }

    /// <inheritdoc />
    public async Task<DispatchedJobDefinition> SaveDispatchedJobDefinitionAsync(
        DispatchedJobDefinition definition,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);

            DispatchedJobs.Add(definition);
        }
        finally
        {
            _semaphore.Release();
        }

        return definition;
    }

    /// <inheritdoc />
    public async Task UpdateJobStateAsync(
        string jobId,
        JobStatus jobStatus,
        DateTime? finishedDate,
        CancellationToken cancellationToken = default
    )
    {
        await UpdateJobAsync(
            jobId,
            job =>
            {
                job.Status = jobStatus;
                job.FinishedAt = finishedDate;
            },
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task UpdateJobRetriesAsync(string jobId, int retries, CancellationToken cancellationToken)
    {
        await UpdateJobAsync(jobId, job => job.Retries = retries, cancellationToken);
    }

    private async Task UpdateJobAsync(
        string jobId,
        Action<DispatchedJobDefinition> updateAction,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            updateAction(DispatchedJobs.First(j => j.Id == jobId));
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
