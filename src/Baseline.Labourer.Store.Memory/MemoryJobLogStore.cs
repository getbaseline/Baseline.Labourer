using Baseline.Labourer.Contracts;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Store.Memory;

public class MemoryJobLogStore : IJobLogStore
{
    private readonly MemoryStore _memoryStore;

    public MemoryJobLogStore(MemoryStore memoryStore)
    {
        _memoryStore = memoryStore;
    }

    public void LogEntryForJob(string jobId, LogLevel logLevel, string message, Exception exception)
    {
        Task
            .Run(async () =>
            {
                using var _ = await _memoryStore.AcquireLockAsync();

                _memoryStore.LogEntries.Add(new MemoryLogEntry
                {
                    JobId = jobId,
                    LogLevel = logLevel,
                    Message = message,
                    Exception = exception
                });
            })
            .ConfigureAwait(false);
    }
}
