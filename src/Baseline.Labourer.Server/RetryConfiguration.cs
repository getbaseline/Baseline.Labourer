using System;
using System.Collections.Generic;
using System.Linq;

namespace Baseline.Labourer.Server;

/// <summary>
/// Configures how retries should be attempted.
/// </summary>
public class RetryConfiguration
{
    /// <summary>
    /// Gets the configured delays for each retry.
    /// </summary>
    public IReadOnlyCollection<TimeSpan> Delays { get; }

    /// <summary>
    /// Gets the number of retries that should be attempted.
    /// </summary>
    public uint Retries { get; }

    public RetryConfiguration(uint retries, TimeSpan delay)
    {
        Retries = retries;
        Delays = Enumerable.Repeat(delay, (int)retries).ToList();
    }

    public RetryConfiguration(uint retries, IReadOnlyCollection<TimeSpan> delays)
    {
        var delaysAsList = delays.ToList();

        // verify an appropriate number of delays are available
        if (delaysAsList.Count != retries)
        {
            throw new ArgumentException(
                "A delay must be provided for each retry when using this constructor. For example, if your retries "
                    + "were set to 3, then you should provide 3 delays. You can set one delay for all retries by using "
                    + "the RetryConfiguration(uint retries, TimeSpan delay) constructor.",
                nameof(delays)
            );
        }

        Retries = retries;
        Delays = delaysAsList;
    }

    /// <summary>
    /// Gets the default retry configuration.
    /// </summary>
    public static RetryConfiguration Default => new RetryConfiguration(3, TimeSpan.FromSeconds(30));
}
