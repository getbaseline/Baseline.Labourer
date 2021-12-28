using System;
using System.Linq;
using Baseline.Labourer.Server.Contracts;

namespace Baseline.Labourer.Server
{
    /// <summary>
    /// A default <see cref="IActivator"/> implementation that expects types to have empty constructors or
    /// constructors that contain Baseline.Labourer provided utilities (such as loggers).
    /// </summary>
    public class DefaultActivator : IActivator
    {
        /// <inheritdoc />
        public object ActivateType(Type type, params object[] overrideParameters)
        {
            var parametersToUse = Array.Empty<object>();

            // If there is only one constructor and that constructor is empty, we can just activate straight away
            // without any dependencies.
            if (type.GetConstructors().Length == 1 && type.GetConstructors().First().GetParameters().Length == 0)
            {
                return Activator.CreateInstance(type);
            }

            // Find the constructor that matches required override parameters.
            foreach (var constructor in type.GetConstructors())
            {
                var constructorParameters = constructor.GetParameters();
                var constructorParameterTypes = constructorParameters.Select(c => c.ParameterType).ToList();

                // If all constructor parameters are contained in our override parameters
                if (constructorParameterTypes.All(cpt => overrideParameters.Any(cpt.IsInstanceOfType)))
                {
                    // Find the first provided override parameter that matches each constructor parameter.
                    parametersToUse = constructorParameterTypes
                        .Select(cpt => overrideParameters.First(cpt.IsInstanceOfType))
                        .ToArray();
                }
            }

            return parametersToUse.Any()
                ? Activator.CreateInstance(type, parametersToUse)
                : Activator.CreateInstance(type);
        }
    }
}