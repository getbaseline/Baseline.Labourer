using System;
using Baseline.Labourer.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Baseline.Labourer.DependencyInjection;

/// <summary>
/// Extension methods of the <see cref="IServiceCollection"/> interface to fluently add the Baseline.Labourer
/// components to the container.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds the Baseline.Labourer client to the service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the client to.</param>
    /// <param name="labourerBuilder">A delegate used to configure the client.</param>
    public static IServiceCollection AddBaselineLabourer(
        this IServiceCollection serviceCollection,
        Action<IServiceProvider, LabourerBuilder> labourerBuilder
    )
    {
        serviceCollection.AddSingleton(
            serviceProvider =>
            {
                var builderInstance = new LabourerBuilder();
                builderInstance.UseLoggerFactoryResolvedFromContainer(serviceProvider);
                labourerBuilder(serviceProvider, builderInstance);

                return builderInstance;
            }
        );

        serviceCollection.AddSingleton<ILabourerClient, LabourerClient>(
            serviceProvider =>
            {
                var b = serviceProvider.GetService<LabourerBuilder>()!;
                return new LabourerClient(b.ToClientConfiguration());
            }
        );

        serviceCollection.AddSingleton(
            serviceProvider =>
            {
                var b = serviceProvider.GetService<LabourerBuilder>()!;
                return new LabourerServer(b.ToServerConfiguration(serviceProvider));
            }
        );

        return serviceCollection;
    }
}
