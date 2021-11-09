using Baseline.Labourer.Server.Contracts;

namespace Baseline.Labourer.Server;

/// <summary>
/// A default <see cref="IJobActivator"/> implementation that expects jobs to have empty constructors or
/// constructors that contain Baseline.Labourer provided utilities (such as loggers).
/// </summary>
public class DefaultJobActivator : IJobActivator
{
    /// <inheritdoc />
    public object ActivateJob(Type jobType, params object[] overrideParameters)
    {
        var parametersToUse = Array.Empty<object>();

        // If there is only one constructor and that constructor is empty, we can just activate straight away
        // without any dependencies.
        if (jobType.GetConstructors().Length == 1 && jobType.GetConstructors().First().GetParameters().Length == 0)
        {
            return Activator.CreateInstance(jobType);
        }

        // Find the constructor that matches required override parameters.
        foreach (var constructor in jobType.GetConstructors())
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
            ? Activator.CreateInstance(jobType, parametersToUse)
            : Activator.CreateInstance(jobType);
    }
}
