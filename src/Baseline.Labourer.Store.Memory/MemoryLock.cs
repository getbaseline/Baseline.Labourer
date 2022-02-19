using System;

namespace Baseline.Labourer
{
    /// <summary>
    /// Represents an in memory lock of a resource.
    /// </summary>
    public class MemoryLock
    {
        /// <summary>
        /// Gets or sets the id of the lock.
        /// </summary>
        public string Id { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets when the lock was released.
        /// </summary>
        public DateTime? Released { get; set; }
        
        /// <summary>
        /// Gets or sets how long the lock is in effect for.
        /// </summary>
        public DateTime? Until { get; set; }
    }
}