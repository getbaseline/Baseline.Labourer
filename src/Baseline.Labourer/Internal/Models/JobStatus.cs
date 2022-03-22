namespace Baseline.Labourer.Internal;

/// <summary>
/// Job status represents each status that a job can be in.
/// </summary>
public enum JobStatus
{
    Unknown,
    Created,
    InProgress,
    Complete,
    Failed,
    FailedExceededMaximumRetries
}
