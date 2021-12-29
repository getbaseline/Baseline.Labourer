using System;
using Baseline.Labourer.Internal.Models;

namespace Baseline.Labourer.Queue.Memory
{
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
    }
}