using Baseline.Labourer.Store.NoOp;

namespace Baseline.Labourer
{
    /// <summary>
    /// Extension methods related to the NoOp store for inheritors of the <see cref="BaseLabourerBuilder"/> class.
    /// </summary>
    public static class BaseLabourerBuilderExtensions
    {
        /// <summary>
        /// Uses the NoOp store as the store of choice within the Baseline.Labourer instance.
        /// </summary>
        /// <param name="builder">The configuration builder to assign the NoOp store to.</param>
        public static T UseNoOpStore<T>(this T builder) where T : BaseLabourerBuilder
        {
            builder.Store = new NoOpStore();
            return builder;
        }
    }
}