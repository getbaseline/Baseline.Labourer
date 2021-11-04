using System;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer
{
    public class BaselineLabourerConfiguration
    {
        /// <summary>
        /// Gets or sets the delegate used to receive an <see cref="ILoggerFactory"/> instance which is used throughout
        /// the client and related projects.
        /// </summary>
        public Func<ILoggerFactory> LoggerFactory { get; set; }
    }
}