using System.Threading.Tasks;

namespace Baseline.Labourer;

/// <summary>
/// A contract that all jobs without parameters and are to be used with Baseline.Labourer implement. Constrains
/// job methods to ensure they don't do anything silly that makes handling them ridiculous like injecting run time
/// dependencies.
/// </summary>
public interface IJob
{
    /// <summary>
    /// Executes the job.
    /// </summary>
    ValueTask HandleAsync();
}
