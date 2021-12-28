using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Tests
{
    public class BasicJob : IJob
    {
        public ValueTask HandleAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}