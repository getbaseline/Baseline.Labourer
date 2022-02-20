using Microsoft.Extensions.DependencyInjection;

namespace Baseline.Labourer
{
    /// <summary>
    /// Extension methods for the <see cref="IServiceCollection"/> interface.
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Adds the <see cref="BaselineLabourerHostedService"/> to the hosted service pool. Still requires the
        /// relevant services to be added to the collection through the Baseline.Labourer.DependencyInjection
        /// library.
        /// </summary>
        /// <param name="serviceCollection">The service collection to modify.</param>
        public static IServiceCollection AddBaselineLabourerServerHostedService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHostedService<BaselineLabourerHostedService>();
            return serviceCollection;
        }
    }
}