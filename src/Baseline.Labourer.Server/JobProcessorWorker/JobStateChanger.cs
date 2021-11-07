using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.JobProcessorWorker
{
    /// <summary>
    /// JobStateChanger is an internal class that provides the ability to modify the state of a job whilst maintaining
    /// all supplemental events (i.e. middleware) required.
    /// </summary>
    public class JobStateChanger
    {
        private readonly string _jobId;
        private readonly IDispatchedJobStore _dispatchedJobStore; 

        public JobStateChanger(string jobId, IDispatchedJobStore dispatchedJobStore)
        {
            _jobId = jobId;
            _dispatchedJobStore = dispatchedJobStore;
        }

        /// <summary>
        /// Changes the state of the job, logging appropriate messages and dispatching registered middleware(s).
        /// </summary>
        /// <param name="status">The new status of the job.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task ChangeStateAsync(JobStatus status, CancellationToken cancellationToken)
        {
            await _dispatchedJobStore.UpdateJobStateAsync(
                _jobId,
                status,
                status == JobStatus.Complete || status == JobStatus.FailedExceededMaximumRetries ? (DateTime?)DateTime.UtcNow : null,
                cancellationToken
            );

            _dispatchedJobStore.LogEntryForJob(_jobId, LogLevel.Information, HumanReadableStatus(status), null);
        }

        private string HumanReadableStatus(JobStatus status) => status switch
        {
            JobStatus.Created => "Job created.",
            JobStatus.InProgress => "Job started.",
            JobStatus.Complete => "Job complete.",
            JobStatus.Failed => "Job failed.",
            _ => "Job has entered an unknown status."
        };
    }
}