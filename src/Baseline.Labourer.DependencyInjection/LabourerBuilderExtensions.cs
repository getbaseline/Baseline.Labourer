using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer
{
    /// <summary>
    /// Extension methods that apply to all inheritors or the <see cref="LabourerBuilder"/> class.
    /// </summary>
    public static class LabourerBuilderExtensions
    {
        /// <summary>
        /// Allows the server to be configured fluently.
        /// </summary>
        /// <param name="builder">The labourer builder to configure.</param>
        /// <param name="serverBuilder">The server builder delegate that configures the server options.</param>
        public static LabourerBuilder ConfigureServer(
            this LabourerBuilder builder,
            Action<LabourerServerBuilder> serverBuilder
        )
        {
            serverBuilder(builder.ServerBuilder);
            return builder;
        }

        /// <summary>
        /// Resolves the <see cref="ILoggerFactory"/> instance from the container and uses that in the Baseline.Labourer
        /// configuration.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <param name="serviceProvider">The service provider to resolve the logger factory from.</param>
        public static LabourerBuilder UseLoggerFactoryResolvedFromContainer(
            this LabourerBuilder builder,
            IServiceProvider serviceProvider
        )
        {
            builder.LoggerFactory = () => serviceProvider.GetService<ILoggerFactory>()!;
            return builder;
        }

        /// <summary>
        /// Assigns the provided <see cref="ILoggerFactory"/> instance to the Baseline.Labourer configuration builder.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <param name="loggerFactory">The logger factory instance.</param>
        public static LabourerBuilder UseThisLoggerFactory(
            this LabourerBuilder builder, 
            ILoggerFactory loggerFactory
        )
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
        public static LabourerBuilder UseThisLoggerFactory(
            this LabourerBuilder builder,
            Func<ILoggerFactory> loggerFactory
        )
        {
            builder.LoggerFactory = loggerFactory;
            return builder;
        }
    }
}