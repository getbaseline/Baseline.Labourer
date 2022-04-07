using System.Threading.Tasks;
using Baseline.Labourer.Internal.Utils;

namespace Baseline.Labourer.Internal.Models;

/// <summary>
/// Model that represents a job in a queue.
/// </summary>
public record QueuedJob
{
    /// <summary>
    /// Gets or sets the id of the message.
    /// </summary>
    public string MessageId { get; init; } = null!;

    /// <summary>
    /// Gets or sets the serialized definition of the job.
    /// </summary>
    public string SerializedDefinition { get; init; } = null!;

    /// <summary>
    /// Deserializes the definition of the queued job into an object and then returns it.
    /// </summary>
    public async Task<T> DeserializeAsync<T>()
    {
        return await SerializationUtils.DeserializeFromStringAsync<T>(SerializedDefinition);
    }
}
