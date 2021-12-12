using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer.Tests
{
    public class BasicJob : IJob
    {
        public Task HandleAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}