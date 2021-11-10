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
        /// <param name="messageToQueue">A message that should be serialized and queued.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task EnqueueAsync<T>(T messageToQueue, CancellationToken cancellationToken);

        /// <summary>
        /// Dequeues and returns a single message from the queue. This method long polls (i.e. waits a specified amount
        /// of time if no messages are available before returning) so there is no need to implement wait times on the
        /// consuming side.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token used to safely shut down the server.</param>
        Task<QueuedJob?> DequeueAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a message from the queue provider.
        /// </summary>
        /// <param name="messageId">The message to remove from the queue.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task DeleteMessageAsync(string messageId, CancellationToken cancellationToken);
    }
}