using System;
using System.Threading.Tasks;

namespace Baseline.Labourer.Tests
{
    public class AssertionUtils
    {
        public static async Task RetryAsync(Action action, int times = 25, int delay = 100)
        {
            for (var i = 0; i < times; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception e)
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
}