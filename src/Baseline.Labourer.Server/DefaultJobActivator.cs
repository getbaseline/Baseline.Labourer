using System;
using Baseline.Labourer.Server.Contracts;

namespace Baseline.Labourer.Server
{
    /// <summary>
    /// A default <see cref="IJobActivator"/> implementation that expects jobs to have empty constructors or
    /// constructors that contain Baseline.Labourer provided utilities (such as loggers).
    /// </summary>
    public class DefaultJobActivator : IJobActivator
    {
        /// <inheritdoc />
        public object ActivateJob(Type jobType, params object[] overrideParameters)
        {
            return Activator.CreateInstance(jobType);
        }
    }
}