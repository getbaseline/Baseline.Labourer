using System;
using Baseline.Labourer.Internal;
using Baseline.Labourer.Internal.Contracts;

namespace Baseline.Labourer.Store.Memory.Tests;

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
