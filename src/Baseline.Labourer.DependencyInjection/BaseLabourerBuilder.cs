using System;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer
{
    /// <summary>
    /// Base class for all interim configuration objects built for dependency injection.
    /// </summary>
    public abstract class BaseLabourerBuilder
    {
        /// <summary>
        /// Gets or sets the logger factory to use.
        /// </summary>
        public Func<ILoggerFactory>? LoggerFactory { get; set; }

        /// <summary>
        /// Gets or sets the queue provider to use.
        /// </summary>
        public IQueue? Queue { get; set; }

        /// <summary>
        /// Gets or sets the store provider to use. 
        /// </summary>
        public IStore? Store { get; set; }
    }
}
