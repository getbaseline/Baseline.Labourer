using System;

namespace Baseline.Labourer.Server.Contracts
{
    /// <summary>
    /// IJobActivator is a contract that defines what any activator (i.e. those creating instances of jobs) must
    /// implement. 
    /// </summary>
    public interface IJobActivator
    {
        /// <summary>
        /// Activates a job from its job type and overrides any parameters that match those provided.
        /// </summary>
        /// <param name="jobType">The type of the job that needs to be activated.</param>
        /// <param name="overrideParameters">Parameters that should override any others.</param>
        object ActivateJob(Type jobType, params object[] overrideParameters);
    }
}