using System.Threading.Tasks;

namespace Baseline.Labourer.Server.Internal;

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