using System.Threading.Tasks;
using Baseline.Labourer.Server.Contracts;

namespace Baseline.Labourer.Server.Internal.BootTasks
{
    /// <summary>
    /// A boot task to bootstrap the chosen store if it's not already been done.
    /// </summary>
    internal class BootstrapStoreBootTask : IBootTask
    {
        /// <inheritdoc />
        public async ValueTask OnBootAsync(ServerContext serverContext)
        {
            await serverContext.Store.BootstrapAsync();
        }
    }
}