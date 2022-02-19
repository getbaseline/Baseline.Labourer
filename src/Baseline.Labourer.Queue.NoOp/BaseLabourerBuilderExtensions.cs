using Baseline.Labourer.DependencyInjection;

namespace Baseline.Labourer
{
    /// <summary>
    /// Extension methods related to the memory queue for inheritors of the <see cref="BaseLabourerBuilder"/> class.
    /// </summary>
    public static class BaseLabourerBuilderExtensions
    {
        /// <summary>
        /// Uses the NoOp queue as the queue of choice within the Baseline.Labourer instance.
        /// </summary>
        /// <param name="builder">The configuration builder to assign the NoOp queue to.</param>
        public static T UseNoOpQueue<T>(this T builder) where T : BaseLabourerBuilder
        {
            builder.Queue = new NoOpQueue();
            return builder;
        }
    }
}