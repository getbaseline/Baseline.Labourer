using System;
using Baseline.Labourer.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Baseline.Labourer
{
    /// <summary>
    /// An <see cref="IActivator"/> implementation that resolves and activates types from the configured service
    /// provider.
    /// </summary>
    public class DependencyInjectionActivator : IActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public DependencyInjectionActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public object ActivateType(Type type, params object[] overrideParameters)
        {
            var scope = _serviceProvider.CreateScope();
            
            // TODO - type the override parameters and only inject them into the constructor when called for.
            return ActivatorUtilities.CreateInstance(scope.ServiceProvider, type);
        }
    }
}