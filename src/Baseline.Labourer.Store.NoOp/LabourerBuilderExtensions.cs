namespace Baseline.Labourer;

/// <summary>
/// Extension methods related to the NoOp store for inheritors of the <see cref="LabourerBuilder"/> class.
/// </summary>
public static class LabourerBuilderExtensions
{
    /// <summary>
    /// Uses the NoOp store as the store of choice within the Baseline.Labourer instance.
    /// </summary>
    /// <param name="builder">The configuration builder to assign the NoOp store to.</param>
    public static LabourerBuilder UseNoOpStore(this LabourerBuilder builder)
    {
        builder.Store = new NoOpStore();
        return builder;
    }
}