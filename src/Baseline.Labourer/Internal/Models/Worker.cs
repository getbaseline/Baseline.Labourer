namespace Baseline.Labourer.Internal;

/// <summary>
/// Worker represents a running worker instance within a server that runs a specific task.
/// </summary>
public class Worker
{
    /// <summary>
    /// Gets or sets the unique id of the worker.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Gets or sets the id of the server that the worker belongs to.
    /// </summary>
    public string ServerInstanceId { get; set; } = null!;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"worker:{Id}";
    }
}