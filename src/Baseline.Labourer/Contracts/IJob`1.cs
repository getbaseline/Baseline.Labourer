using System.Threading.Tasks;

namespace Baseline.Labourer;

/// <summary>
/// A contract that all jobs that have parameters and are to be used with Baseline.Labourer implement. Constrains
/// job methods to ensure they don't do anything silly that makes handling them ridiculous like injecting run time
/// dependencies.
/// </summary>
/// <typeparam name="TParams">
/// The type of job parameters, usually a POCO used to configure the job to do what it should do.
/// </typeparam>
public interface IJob<in TParams>
{
    /// <summary>
    /// Executes the job.
    /// </summary>
    /// <param name="parameters">The job's parameters.</param>
    Task HandleAsync(TParams parameters);
}
