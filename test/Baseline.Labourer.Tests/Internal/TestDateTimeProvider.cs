using System;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Contracts;

namespace Baseline.Labourer.Tests.Internal;

public class TestDateTimeProvider : IDateTimeProvider
{
    private Func<DateTime> _dateTimer = () => DateTime.UtcNow;

    public DateTime UtcNow()
    {
        return _dateTimer();
    }

    public void SetUtcNow(DateTime dateTime)
    {
        _dateTimer = () => dateTime;
    }
}
