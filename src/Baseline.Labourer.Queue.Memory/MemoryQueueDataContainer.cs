using System.Collections.Generic;

namespace Baseline.Labourer
{
    /// <summary>
    /// A class containing the underlying data of the in memory queue. Used to share this data between disparate
    /// client and server registrations.
    /// </summary>
    public class MemoryQueueDataContainer
    {
        public List<MemoryQueuedJob> Queue { get; } = new List<MemoryQueuedJob>();
        public List<MemoryQueuedJob> RemovedQueue { get; } = new List<MemoryQueuedJob>();
    }
}