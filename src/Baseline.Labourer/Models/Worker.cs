namespace Baseline.Labourer;

/// <summary>
/// Worker represents a running worker instance within a server that runs a specific task.
/// </summary>
public class Worker
{
    /// <summary>
    /// Gets or sets the unique id of the worker.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the id of the server that the worker belongs to.
    /// </summary>
    public string ServerInstanceId { get; set; }

    /// <summary>
    /// Gets or sets the type of the worker.
    /// </summary>
    public WorkerType Type { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"worker:{Id}";
    }
}
