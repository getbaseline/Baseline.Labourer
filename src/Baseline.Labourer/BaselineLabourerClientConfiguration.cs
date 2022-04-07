using System;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Contracts;
using Microsoft.Extensions.Logging;

namespace Baseline.Labourer;

public class BaselineLabourerClientConfiguration
{
    /// <summary>
    /// Gets or sets a date time provider to use within the client. Chances are you won't need to change this, so don't!
    /// </summary>
    public IDateTimeProvider DateTimeProvider { get; set; } = new DateTimeProvider();

    /// <summary>
    /// Gets or sets the delegate used to receive an <see cref="ILoggerFactory"/> instance which is used throughout
    /// the client and related projects.
    /// </summary>
    public Func<ILoggerFactory>? LoggerFactory { get; set; }

    /// <summary>
    /// Gets or sets the queue provider to use.
    /// </summary>
    public IQueue Queue { get; set; } = null!;

    /// <summary>
    /// Gets or sets the store provider to use.
    /// </summary>
    public IStore Store { get; set; } = null!;
}
