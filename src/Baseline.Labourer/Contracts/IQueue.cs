using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer
{
    /// <summary>
    /// A contract that defines what all queue implementations for Baseline.Labourer must implement.
    /// </summary>
    public interface IQueue
    {
        /// <summary>
        /// Enqueues a message to the queue.
        /// </summary>
        /// <param name="queuedMessageType">The type of the message being queued.</param>
        /// <param name="messageToQueue">A message that should be serialized and queued.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task EnqueueAsync<T>(QueuedMessageType queuedMessageType, T messageToQueue, CancellationToken cancellationToken);

        /// <summary>
        /// Dequeues and returns a single message from the queue.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task<QueuedJob> DequeueAsync(CancellationToken cancellationToken);
    }
}