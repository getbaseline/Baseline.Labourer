namespace Baseline.Labourer.Internal.Models;

/// <summary>
/// Server represents a single running instance of the Baseline.Labourer.Server project.
/// </summary>
public record ServerInstance
{
    /// <summary>
    /// Gets the unique id of the server.
    /// </summary>
    public string Id => $"{Hostname}-{Key}";

    /// <summary>
    /// Gets or sets the hostname of the server that the server instance is running on.
    /// </summary>
    public string Hostname { get; init; } = null!;

    /// <summary>
    /// Gets or sets the uniquely identifitable key of the server.
    /// </summary>
    public string Key { get; init; } = null!;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"server:{Id}";
    }
}
