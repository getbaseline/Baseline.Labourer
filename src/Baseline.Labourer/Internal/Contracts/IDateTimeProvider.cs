using System;

namespace Baseline.Labourer.Internal.Contracts;

/// <summary>
/// IDateTimeProvider is an abstraction used to ensure tests can work without waiting 73 billion years for scheduled
/// tests to run.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current date and time in UTC.
    /// </summary>
    DateTime UtcNow();
}
