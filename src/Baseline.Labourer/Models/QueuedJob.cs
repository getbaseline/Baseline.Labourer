using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer
{
    /// <summary>
    /// Model that represents a job in a queue.
    /// </summary>
    public class QueuedJob
    {
        /// <summary>
        /// Gets or sets the type of queued job that this is.
        /// </summary>
        public QueuedMessageType Type { get; set; }
        
        /// <summary>
        /// Gets or sets the serialized definition of the job.
        /// </summary>
        public string SerializedDefinition { get; set; }

        /// <summary>
        /// Deserializes the definition of the queued job into an object and then returns it.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task<T> DeserializeAsync<T>(CancellationToken cancellationToken)
        {
            return default;
        }
    }
}