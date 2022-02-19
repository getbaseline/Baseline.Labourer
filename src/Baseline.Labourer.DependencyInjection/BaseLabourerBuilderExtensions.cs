using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer.DependencyInjection
{
    /// <summary>
    /// Extension methods that apply to all inheritors or the <see cref="BaseLabourerBuilder"/> class.
    /// </summary>
    public static class BaseLabourerBuilderExtensions
    {
        /// <summary>
        /// Resolves the <see cref="ILoggerFactory"/> instance from the container and uses that in the Baseline.Labourer
        /// configuration.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <param name="serviceProvider">The service provider to resolve the logger factory from.</param>
        public static T UseLoggerFactoryFromContainer<T>(
            this T builder,
            IServiceProvider serviceProvider
        ) where T : BaseLabourerBuilder
        {
            builder.LoggerFactory = () => serviceProvider.GetService<ILoggerFactory>();
            return builder;
        }

        /// <summary>
        /// Assigns the provided <see cref="ILoggerFactory"/> instance to the Baseline.Labourer configuration builder.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <param name="loggerFactory">The logger factory instance.</param>
        public static T UseDefinedLoggerFactory<T>(
            this T builder, 
            ILoggerFactory loggerFactory
        ) where T : BaseLabourerBuilder
        {
            builder.LoggerFactory = () => loggerFactory;
            return builder;
        }

        /// <summary>
        /// Assigns the provided <see cref="ILoggerFactory"/> yielding delegate to the Baseline.Labourer configuration
        /// builder.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <param name="loggerFactory">A delegate that yields a logger factory instance.</param>
        public static T UseDefinedLoggerFactory<T>(
            this T builder,
            Func<ILoggerFactory> loggerFactory
        ) where T : BaseLabourerBuilder
        {
            builder.LoggerFactory = loggerFactory;
            return builder;
        }
    }
}