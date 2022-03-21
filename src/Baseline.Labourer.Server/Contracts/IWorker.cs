using System.Threading.Tasks;

namespace Baseline.Labourer.Server;

/// <summary>
/// IWorker defines what all server workers must implement.
/// </summary>
public interface IWorker
{
    /// <summary>
    /// Runs the worker as a long running task.
    /// </summary>
    Task RunAsync();
}