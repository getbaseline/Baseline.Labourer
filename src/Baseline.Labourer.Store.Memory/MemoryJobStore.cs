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

        public void LogEntryForJob(string jobId, LogLevel logLevel, string message, Exception? exception)
        {
            LogEntries.Add(new LogEntry
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
}