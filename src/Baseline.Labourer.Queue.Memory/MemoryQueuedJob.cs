using System;
using Baseline.Labourer.Internal;

namespace Baseline.Labourer;

/// <summary>
/// A queued job representation with additional information related to processing them in memory.
/// </summary>
public class MemoryQueuedJob : QueuedJob
{
    /// <summary>
    /// Gets or sets the visibility delay of the memory queued job.
    /// </summary>
    public TimeSpan? VisibilityDelay { get; set; }

    /// <summary>
    /// Gets or sets when the message was enqueued.
    /// </summary>
    public DateTime EnqueuedAt { get; set; }

    /// <summary>
    /// Gets or sets the previous visibility delay (if there is one). This is used to monitor the visibility delay
    /// and perform assertions when the message was enqueued because, once the message is read, it's automatically
    /// hidden for a period of time to prevent any additional workers from querying it.
    /// </summary>
    public TimeSpan? PreviousVisibilityDelay { get; set; }
}
