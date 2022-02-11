using System.Threading.Tasks;
using Baseline.Labourer.Server.Contracts;

namespace Baseline.Labourer.Server.Internal.BootTasks
{
    /// <summary>
    /// A boot task to bootstrap the chosen queue if it's not already been done.
    /// </summary>
    internal class BootstrapQueueBootTask : IBootTask
    {
        /// <inheritdoc />
        public async ValueTask OnBootAsync(ServerContext serverContext)
        {
            await serverContext.Queue.BootstrapAsync();
        }
    }
}