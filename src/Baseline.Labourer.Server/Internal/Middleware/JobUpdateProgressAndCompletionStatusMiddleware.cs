using System;
using System.Threading.Tasks;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Models;

namespace Baseline.Labourer.Server.Internal.Middleware;

/// <summary>
/// Middleware for updating the status of the relevant job based on events that occur.
/// </summary>
internal class JobUpdateProgressAndCompletionStatusMiddleware : JobMiddleware
{
    /// <summary>
    /// Updates the job's status to mark it as completed.
    /// </summary>
    /// <param name="jobContext">The job's context.</param>
    public override async ValueTask JobCompletedAsync(JobContext jobContext)
    {
        await using var writer = jobContext.BeginTransaction();
        await jobContext.UpdateJobStateAsync(writer, JobStatus.Complete);
        await writer.CommitAsync();
    }

    /// <summary>
    /// Updates the job's status to mark it as in progress.
    /// </summary>
    /// <param name="jobContext">The job's context.</param>
    public override async ValueTask JobStartedAsync(JobContext jobContext)
    {
        await using var writer = jobContext.BeginTransaction();
        await jobContext.UpdateJobStateAsync(writer, JobStatus.InProgress);
        await writer.CommitAsync();
    }

    /// <summary>
    /// Updates the job's status to mark it as failed and that it has exceeded all retries.
    /// </summary>
    /// <param name="jobContext">The jobs context.</param>
    /// <param name="exception">The exception (if there was one).</param>
    public override async ValueTask JobFailedAndExceededRetriesAsync(
        JobContext jobContext,
        Exception? exception
    )
    {
        await using var writer = jobContext.BeginTransaction();
        await jobContext.UpdateJobStateAsync(writer, JobStatus.FailedExceededMaximumRetries);
        await writer.CommitAsync();
    }
}
