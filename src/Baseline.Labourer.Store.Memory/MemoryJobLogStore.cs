using Baseline.Labourer.Contracts;
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
        private readonly MemoryStore _memoryStore;

        public MemoryJobLogStore(MemoryStore memoryStore)
        {
            _memoryStore = memoryStore;
        }

        /// <inheritdoc />
        public void LogEntryForJob(string jobId, LogLevel logLevel, string message, Exception exception)
        {
            Task
                .Run(async () =>
                {
                    using var _ = await _memoryStore.AcquireStoreLockAsync();

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
}