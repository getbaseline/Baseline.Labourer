using System;
using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer
{
    /// <summary>
    /// Provides a way to lock resources ensuring multiple workers do not perform the same action. This is naturally
    /// not completely fool proof, but is a good step towards it. 
    /// </summary>
    public interface IResourceLocker
    {
        /// <summary>
        /// Locks a resource. This function returns an async disposable meaning it should be used with a USING
        /// statement to ensure the lock is released correctly. The for date provided is to ensure that a lock will
        /// automatically release once a certain amount of time has passed. An exception will be thrown if the resource
        /// is already locked. 
        /// </summary>
        /// <param name="resource">The resource identifier to lock.</param>
        /// <param name="for">The duration of time to lock the record for if it is not released manually.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task<IAsyncDisposable> LockResourceAsync(string resource, TimeSpan @for, CancellationToken cancellationToken);
    }
}