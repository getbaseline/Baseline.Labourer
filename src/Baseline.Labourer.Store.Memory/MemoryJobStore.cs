using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Store.Memory
{
    public class MemoryJobStore : IDispatchedJobStore
    {
        protected class LogEntry
        {
            public string JobId { get; set; }
            public LogLevel LogLevel { get; set; }
            public string Message { get; set; }
            public Exception? Exception { get; set; }
        }
        
        protected readonly List<DispatchedJobDefinition> DispatchedJobs = new List<DispatchedJobDefinition>();
        protected readonly List<LogEntry> LogEntries = new List<LogEntry>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public Task LogEntryForJob(string jobId, LogLevel logLevel, string message, Exception? exception)
        {
            LogEntries.Add(new LogEntry
            {
                JobId = jobId,
                LogLevel = logLevel,
                Message = message,
                Exception = exception
            });
            return Task.CompletedTask;
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
        public async Task UpdateJobAsync(
            string jobId, 
            DispatchedJobDefinition jobDefinition, 
            CancellationToken cancellationToken
        )
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);

                var relevantDispatchedJob = DispatchedJobs.FirstOrDefault(j => j.Id == jobId);
                if (relevantDispatchedJob == null)
                {
                    return;
                }

                relevantDispatchedJob.Status = jobDefinition.Status;
                relevantDispatchedJob.FinishedAt = jobDefinition.FinishedAt;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}