using System;
using System.Linq;
using Baseline.Labourer.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Baseline.Labourer.DependencyInjection;

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
        var amountOfConstructors = type.GetConstructors().Length;

        if (amountOfConstructors == 0)
        {
            return ActivatorUtilities.CreateInstance(scope.ServiceProvider, type);
        }

        if (amountOfConstructors > 1)
        {
            throw new InvalidOperationException(
                $"The {nameof(DependencyInjectionActivator)} only supports classes with none or one constructor."
            );
        }

        var applicableOverrideParameters = overrideParameters.Where(
            o =>
                type.GetConstructors()
                    .First()
                    .GetParameters()
                    .Any(p => p.ParameterType.IsInstanceOfType(o))
        );

        return ActivatorUtilities.CreateInstance(
            scope.ServiceProvider,
            type,
            applicableOverrideParameters.ToArray()
        );
    }
}
