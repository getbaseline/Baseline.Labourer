﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Server.Internal
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
        /// <param name="jobId">The id of the job to have its state changed.</param>
        /// <param name="status">The new status of the job.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task ChangeStateAsync(JobStatus status, CancellationToken cancellationToken)
        {
            await _dispatchedJobStore.UpdateJobAsync(
                _jobId,
                new DispatchedJobDefinition
                {
                    Status = status,
                    FinishedAt = status == JobStatus.Complete || status == JobStatus.Failed ? (DateTime?) DateTime.UtcNow : null
                },
                cancellationToken
            );
        }
    }
}