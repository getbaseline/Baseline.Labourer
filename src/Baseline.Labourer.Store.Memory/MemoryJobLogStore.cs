using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Baseline.Labourer.Store.Memory
{
    /// <summary>
    /// Provides a log store implementation for the memory store project.
    /// </summary>
    public class MemoryJobLogStore : IJobLogStore
    {
        private readonly MemoryBackingStore _memoryBackingStore;

        public MemoryJobLogStore(MemoryBackingStore memoryBackingStore)
        {
            _memoryBackingStore = memoryBackingStore;
        }

        /// <inheritdoc />
        public void LogEntryForJob(string jobId, LogLevel logLevel, string message, Exception? exception)
        {
            Task
                .Run(async () =>
                {
                    using var _ = await _memoryBackingStore.AcquireStoreLockAsync();

                    _memoryBackingStore.LogEntries.Add(new MemoryLogEntry
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
}