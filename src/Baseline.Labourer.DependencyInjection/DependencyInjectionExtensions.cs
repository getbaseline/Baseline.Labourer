﻿using System;
using Microsoft.Extensions.DependencyInjection;

namespace Baseline.Labourer
{
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
        /// <param name="builder">A delegate used to configure the client.</param>
        public static IServiceCollection AddBaselineLabourerClient(
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, LabourerClientBuilder> builder
        )
        {
            return serviceCollection.AddSingleton<ILabourerClient, LabourerClient>(serviceProvider =>
            {
                var b = new LabourerClientBuilder();
                builder(serviceProvider, b);

                return new LabourerClient(b.ToConfiguration());
            });
        }

        /// <summary>
        /// Adds the Baseline.Labourer server to the service collection.
        /// </summary>
        /// <param name="serviceCollection">The service collection to add the server to.</param>
        /// <param name="builder">A delegate used to configure the server.</param>
        public static IServiceCollection AddBaselineLabourerServer(
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, LabourerClientBuilder> builder
        )
        {
            return serviceCollection;
        }
    }
}