using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Tests
{
    public class BasicSuccessfulJob : IJob
    {
        public ValueTask HandleAsync(CancellationToken cancellationToken)
        {
            return new ValueTask();
        }
    }
}