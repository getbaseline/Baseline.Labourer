using System.Threading;
using System.Threading.Tasks;

namespace Baseline.Labourer
{
    /// <summary>
    /// A contract that all jobs to be used with Baseline.Labourer implement. Constrains job methods to ensure they
    /// don't do anything silly that makes handling them ridiculous.
    /// </summary>
    /// <typeparam name="TParams">
    /// The type of job parameters, usually a POCO used to configure the job to do what it should do.
    /// </typeparam>
    public interface IJob<in TParams>
    {
        Task HandleAsync(TParams parameters, CancellationToken cancellationToken);
    }
}