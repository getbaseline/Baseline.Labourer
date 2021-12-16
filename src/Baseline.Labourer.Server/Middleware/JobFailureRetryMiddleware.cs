using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Server.Contracts;
using Baseline.Labourer.Server.JobProcessorWorker;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.Middleware
{
    /// <summary>
    /// JobFailureRetryMiddleware is a middleware for retrying failed jobs. It determines whether or not they
    /// have exceeded their maximum amount of retries and, if they have not, schedules them to be executed again.
    /// </summary>
    public class JobFailureRetryMiddleware : JobMiddleware
    {
        /// <inheritdoc />
        public override async ValueTask JobFailedAsync(
            JobContext jobContext, 
            Exception? exception, 
            CancellationToken cancellationToken
        )
        {
            var jobStoredLogger = new JobLoggerFactory(jobContext).CreateLogger<JobFailureRetryMiddleware>();
            
            jobStoredLogger.LogError(jobContext, "Job failed.", exception);
            
            await using var writer = jobContext.BeginTransaction();
            
            // We only want to retry this job if it's not exceeded its retry limit.
            if (jobContext.JobDefinition.Retries >= 3)
            { 
                jobStoredLogger.LogError(
                    jobContext,
                    "Job has exceeded its maximum amount of retries. Marking job as failed and exceeded maximum retries."
                );

                await jobContext.UpdateJobStateAsync(writer, JobStatus.FailedExceededMaximumRetries, cancellationToken);
                await writer.CommitAsync(cancellationToken);

                return;
            }
            
            jobStoredLogger.LogInformation(
                jobContext,
                $"Retrying job. Attempt {jobContext.JobDefinition.Retries + 1} of 3."
            );

            await jobContext.UpdateJobStateAsync(writer, JobStatus.Failed, cancellationToken);
            await jobContext.IncrementJobRetriesAsync(writer, cancellationToken);
            await jobContext.RequeueJobAsync(cancellationToken);

            await writer.CommitAsync(cancellationToken);
        }
    }
}