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
        /// <param name="dataContainer">
        /// The data container used to share queue information between client and server registrations.
        /// </param>
        public static T UseMemoryQueue<T>(this T builder, MemoryQueueDataContainer dataContainer) where T : BaseLabourerBuilder
        {
            builder.Queue = new MemoryQueue(dataContainer);
            return builder;
        }
    }
}