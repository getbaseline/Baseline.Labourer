using System;

namespace Baseline.Labourer.Internal;

/// <summary>
/// Standard implementation of the <see cref="IDateTimeProvider"/> interface.
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    /// <summary>
    /// Gets the current date and time in UTC format.
    /// </summary>
    public DateTime UtcNow()
    {
        return DateTime.UtcNow;
    }
}