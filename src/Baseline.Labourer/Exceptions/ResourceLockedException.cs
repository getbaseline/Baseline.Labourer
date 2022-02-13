using System;

namespace Baseline.Labourer.Exceptions
{
    /// <summary>
    /// An exception that is thrown when a resource is locked and any attempts to wait for an unlock failed.
    /// </summary>
    public class ResourceLockedException : Exception
    {
        /// <summary>
        /// Gets the resource that was locked.
        /// </summary>
        public string ResourceId { get; }

        public ResourceLockedException(string resourceId) : base($"The resource {resourceId} is locked.")
        {
            ResourceId = resourceId;
        }
    }
}