using System;
using System.Threading.Tasks;

namespace Baseline.Labourer.Tests.Scenarios.Internal;

public class AssertionUtils
{
    public static async Task EnsureAsync(Action action, int times = 10, int delay = 100)
    {
        for (var i = 0; i < times; i++)
        {
            action();
            await Task.Delay(delay);
        }
    }

    public static async Task RetryAsync(Action action, int times = 25, int delay = 100)
    {
        for (var i = 0; i < times; i++)
        {
            try
            {
                action();
                return;
            }
            catch (Exception)
            {
                if (i == (times - 1))
                {
                    throw;
                }
            }

            await Task.Delay(delay);
        }
    }

    public static async Task RetryAsync(Func<Task> action, int times = 25, int delay = 100)
    {
        for (var i = 0; i < times; i++)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception)
            {
                if (i == (times - 1))
                {
                    throw;
                }
            }

            await Task.Delay(delay);
        }
    }
}
