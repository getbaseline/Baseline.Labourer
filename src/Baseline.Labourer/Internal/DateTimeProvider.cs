using System;
using Baseline.Labourer.Internal.Contracts;

namespace Baseline.Labourer.Internal
{
    /// <summary>
    /// Standard implementation of the <see cref="IDateTimeProvider"/> interface.
    /// </summary>
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}