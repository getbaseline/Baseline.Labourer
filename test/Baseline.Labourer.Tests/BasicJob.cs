using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Tests;

public class BasicJob : IJob
{
    public ValueTask HandleAsync()
    {
        throw new System.NotImplementedException();
    }
}
