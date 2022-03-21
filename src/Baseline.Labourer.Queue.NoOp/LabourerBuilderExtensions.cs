namespace Baseline.Labourer;

/// <summary>
/// Extension methods related to the memory queue for inheritors of the <see cref="LabourerBuilder"/> class.
/// </summary>
public static class LabourerBuilderExtensions
{
    /// <summary>
    /// Uses the NoOp queue as the queue of choice within the Baseline.Labourer instance.
    /// </summary>
    /// <param name="builder">The configuration builder to assign the NoOp queue to.</param>
    public static LabourerBuilder UseNoOpQueue(this LabourerBuilder builder)
    {
        builder.Queue = new NoOpQueue();
        return builder;
    }
}