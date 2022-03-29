using System.Threading.Tasks;

namespace Baseline.Labourer.Server;

/// <summary>
/// Defines a contract that all boot tasks must implement. Boot tasks are things that are ran on the boot of the
/// server, for example bootstrapping the job store.
/// </summary>
internal interface IBootTask
{
    /// <summary>
    /// Runs a boot task.
    /// </summary>
    /// <param name="serverContext">The server context the boot task is running within.</param>
    ValueTask OnBootAsync(ServerContext serverContext);
}
