using System;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Models;
using Baseline.Labourer.Server.Internal.JobProcessorWorker;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.Internal.Middleware;

/// <summary>
/// A middleware for retrying failed jobs. It determines whether or not they have exceeded their maximum amount of
/// retries and, if they have not, schedules them to be executed again.
/// </summary>
internal class JobFailureRetryMiddleware : JobMiddleware
{
    /// <inheritdoc />
    public override async ValueTask<MiddlewareContinuation> JobFailedAsync(
        JobContext jobContext,
        Exception? exception
    )
    {
        var jobStoredLogger = new JobLoggerFactory(
            jobContext
        ).CreateLogger<JobFailureRetryMiddleware>();
        var retryCountForJob = jobContext.RetryCountForJob();

        jobStoredLogger.LogError(jobContext, "Job failed.", exception);

        // We only want to retry this job if it's not exceeded its retry limit.
        if (jobContext.JobDefinition.Retries >= retryCountForJob)
        {
            jobStoredLogger.LogDebug(
                jobContext,
                "Job has exceeded its maximum amount of retries. Marking job as failed and exceeded maximum retries."
            );

            await new JobMiddlewareRunner(
                jobContext.WorkerContext.ServerContext
            ).JobFailedAndExceededRetriesAsync(jobContext, exception);

            return MiddlewareContinuation.Abort;
        }

        jobStoredLogger.LogDebug(
            jobContext,
            "Retrying job. Attempt {currentRetries} of {allowedRetries}.",
            jobContext.JobDefinition.Retries + 1,
            retryCountForJob
        );

        await using var writer = jobContext.BeginTransaction();
        await jobContext.UpdateJobStateAsync(writer, JobStatus.Failed);
        await jobContext.IncrementJobRetriesAsync(writer);
        await writer.CommitAsync();

        await jobContext.RequeueJobAsync();

        return MiddlewareContinuation.Continue;
    }
}
