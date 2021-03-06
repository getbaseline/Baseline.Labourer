namespace Baseline.Labourer.Internal.Models;

/// <summary>
/// Worker represents a running worker instance within a server that runs a specific task.
/// </summary>
public record Worker
{
    /// <summary>
    /// Gets or sets the unique id of the worker.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Gets or sets the id of the server that the worker belongs to.
    /// </summary>
    public string ServerInstanceId { get; init; } = null!;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"worker:{Id}";
    }
}
