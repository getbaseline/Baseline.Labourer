using Baseline.Labourer.DependencyInjection;

namespace Baseline.Labourer
{
    /// <summary>
    /// Extension methods related to the memory queue for inheritors of the <see cref="BaseLabourerBuilder"/> class.
    /// </summary>
    public static class BaseLabourerBuilderExtensions
    {
        /// <summary>
        /// Uses the memory queue as the queue of choice within the Baseline.Labourer instance.
        /// </summary>
        /// <param name="builder">The configuration builder to assign the memory queue to.</param>
        public static T UseMemoryQueue<T>(this T builder) where T : BaseLabourerBuilder
        {
            builder.Queue = new MemoryQueue();
            return builder;
        }
    }
}