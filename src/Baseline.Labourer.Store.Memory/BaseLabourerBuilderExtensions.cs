using Baseline.Labourer.DependencyInjection;

namespace Baseline.Labourer
{
    /// <summary>
    /// Extension methods related to the memory store for inheritors of the <see cref="BaseLabourerBuilder"/> class.
    /// </summary>
    public static class BaseLabourerBuilderExtensions
    {
        /// <summary>
        /// Uses the memory store as the store of choice within the Baseline.Labourer instance.
        /// </summary>
        /// <param name="builder">The configuration builder to assign the memory store to.</param>
        public static T UseMemoryStore<T>(this T builder) where T : BaseLabourerBuilder
        {
            builder.Store = new MemoryStore();
            return builder;
        }
    }
}