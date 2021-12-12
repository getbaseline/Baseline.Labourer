using System;

namespace Baseline.Labourer.Store.Memory
{
    /// <summary>
    /// Represents an in memory lock of a resource.
    /// </summary>
    public class MemoryLock
    {
        /// <summary>
        /// Gets or sets the id of the lock.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets whether this lock is in effect or not.
        /// </summary>
        public bool Locked => Released == null;
        
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