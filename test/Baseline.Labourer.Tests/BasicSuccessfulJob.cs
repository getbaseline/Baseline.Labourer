using System.Threading.Tasks;

namespace Baseline.Labourer.Tests;

public class BasicSuccessfulJob : IJob
{
    public ValueTask HandleAsync()
    {
        return new ValueTask();
    }
}
