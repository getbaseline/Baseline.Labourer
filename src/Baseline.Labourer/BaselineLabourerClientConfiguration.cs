using System;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer;

public class BaselineLabourerClientConfiguration
{
    /// <summary>
    /// Gets or sets the delegate used to receive an <see cref="ILoggerFactory"/> instance which is used throughout
    /// the client and related projects.
    /// </summary>
    public Func<ILoggerFactory>? LoggerFactory { get; set; }

    /// <summary>
    /// Gets or sets the queue provider to use.
    /// </summary>
    public IQueue Queue { get; set; }

    /// <summary>
    /// Gets or sets the store provider to use.
    /// </summary>
    public IStore Store { get; set; }
}
