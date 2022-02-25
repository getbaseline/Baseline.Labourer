using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Baseline.Labourer
{
    /// <summary>
    /// Provides a log store implementation for the memory store project.
    /// </summary>
    public class MemoryJobLogStore : IJobLogStore
    {
        private readonly MemoryStoreDataContainer _memoryStoreDataContainer;

        public MemoryJobLogStore(MemoryStoreDataContainer memoryStoreDataContainer)
        {
            _memoryStoreDataContainer = memoryStoreDataContainer;
        }

        /// <inheritdoc />
        public void LogEntryForJob(string jobId, LogLevel logLevel, string message, Exception? exception)
        {
            Task
                .Run(async () =>
                {
                    using var _ = await _memoryStoreDataContainer.AcquireStoreLockAsync();

                    _memoryStoreDataContainer.LogEntries.Add(new MemoryLogEntry
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