using System;

namespace Baseline.Labourer.Server
{
    /// <summary>
    /// IActivator is a contract that defines what any activator (i.e. those creating instances of jobs, middleware etc)
    /// must implement. 
    /// </summary>
    public interface IActivator
    {
        /// <summary>
        /// Activates a type and overrides any parameters that match those provided.
        /// </summary>
        /// <param name="type">The type that needs to be activated.</param>
        /// <param name="overrideParameters">Parameters that should override any others.</param>
        object ActivateType(Type type, params object[] overrideParameters);
    }
}