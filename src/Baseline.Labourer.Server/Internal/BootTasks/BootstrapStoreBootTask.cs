using System.Threading.Tasks;

namespace Baseline.Labourer.Server.Internal;

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