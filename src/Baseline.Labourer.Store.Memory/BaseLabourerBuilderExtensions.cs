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
        /// <param name="dataContainer">
        /// The data container used to share store information between client and server registrations.
        /// </param>
        public static T UseMemoryStore<T>(this T builder, MemoryStoreDataContainer dataContainer) where T : BaseLabourerBuilder
        {
            builder.Store = new MemoryStore(dataContainer);
            return builder;
        }
    }
}