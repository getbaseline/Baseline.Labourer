using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.Server.Internal;

/// <summary>
/// A middleware for retrying failed jobs. It determines whether or not they have exceeded their maximum amount of
/// retries and, if they have not, schedules them to be executed again.
/// </summary>
internal class JobFailureRetryMiddleware : JobMiddleware
{
    /// <inheritdoc />
    public override async ValueTask<MiddlewareContinuation> JobFailedAsync(
        JobContext jobContext,
        Exception? exception,
        CancellationToken cancellationToken
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
            ).JobFailedAndExceededRetriesAsync(jobContext, exception, cancellationToken);

            return MiddlewareContinuation.Abort;
        }

        jobStoredLogger.LogDebug(
            jobContext,
            "Retrying job. Attempt {currentRetries} of {allowedRetries}.",
            jobContext.JobDefinition.Retries + 1,
            retryCountForJob
        );

        await using var writer = jobContext.BeginTransaction();

        await jobContext.UpdateJobStateAsync(writer, JobStatus.Failed, cancellationToken);
        await jobContext.IncrementJobRetriesAsync(writer, cancellationToken);
        await jobContext.RequeueJobAsync(cancellationToken);

        await writer.CommitAsync(cancellationToken);

        return MiddlewareContinuation.Continue;
    }
}
