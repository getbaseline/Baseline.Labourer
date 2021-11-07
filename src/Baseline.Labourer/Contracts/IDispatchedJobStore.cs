using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer
{
    /// <summary>
    /// A contract that defines what all dispatched job stores must implement. This is a "slimmed down" interface of
    /// what job stores implement applicable to only immediately dispatched jobs.
    /// </summary>
    public interface IDispatchedJobStore
    {
        /// <summary>
        /// Creates and saves a log entry against a job specified by the job id parameter.
        /// </summary>
        /// <param name="jobId">The id of the job that should have a log entry created against it.</param>
        /// <param name="logLevel">The logging level, i.e. the severity of the log.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">An optional exception, if there was one present.</param>
        void LogEntryForJob(
            string jobId, 
            LogLevel logLevel, 
            string message, 
            Exception? exception
        );
        
        /// <summary>
        /// Saves a dispatched job to the job store.
        /// </summary>
        /// <param name="definition">The definition object.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task<DispatchedJobDefinition> SaveDispatchedJobDefinitionAsync(
            DispatchedJobDefinition definition, 
            CancellationToken cancellationToken
        );

        /// <summary>
        /// Updates the state of a job.
        /// </summary>
        /// <param name="jobId">The id of the job to update the state of.</param>
        /// <param name="jobStatus">The status to set against the job.</param>
        /// <param name="finishedDate">The date the job finished (if the status is complete or failed).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task UpdateJobStateAsync(
            string jobId, 
            JobStatus jobStatus, 
            DateTime? finishedDate, 
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Updates the persisted retries for a job.
        /// </summary>
        /// <param name="jobId">The job id to update the retries for.</param>
        /// <param name="retries">The retries to save against the job.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task UpdateJobRetriesAsync(string jobId, int retries, CancellationToken cancellationToken);
    }
}