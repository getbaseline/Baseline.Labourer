namespace Baseline.Labourer
{
    /// <summary>
    /// Extension methods related to the memory store for inheritors of the <see cref="LabourerBuilder"/> class.
    /// </summary>
    public static class LabourerBuilderExtensions
    {
        /// <summary>
        /// Uses the memory store as the store of choice within the Baseline.Labourer instance.
        /// </summary>
        /// <param name="builder">The configuration builder to assign the memory store to.</param>
        /// <param name="dataContainer">
        /// The data container used to share store information between client and server registrations.
        /// </param>
        public static LabourerBuilder UseMemoryStore(this LabourerBuilder builder)
        {
            builder.Store = new MemoryStore();
            return builder;
        }
    }
}